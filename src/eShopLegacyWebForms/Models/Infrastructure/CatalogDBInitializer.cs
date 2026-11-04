using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eShopLegacyWebForms.Models.Infrastructure
{
    public class CatalogDBInitializer : CreateDatabaseIfNotExists<CatalogDBContext>
    {
        private const string DBCatalogSequenceName = "catalog_type_hilo";
        private const string DBBrandSequenceName = "catalog_brand_hilo";
        private const string CatalogItemHiLoSequenceScript = @"Models\Infrastructure\dbo.catalog_hilo.Sequence.sql";
        private const string CatalogBrandHiLoSequenceScript = @"Models\Infrastructure\dbo.catalog_brand_hilo.Sequence.sql";
        private const string CatalogTypeHiLoSequenceScript = @"Models\Infrastructure\dbo.catalog_type_hilo.Sequence.sql";

        private readonly IAmazonS3 _s3Client;
        private readonly string _s3BucketName;
        private readonly string _s3KeyPrefix;

        private CatalogItemHiLoGenerator indexGenerator;
        private bool useCustomizationData;

        public CatalogDBInitializer(CatalogItemHiLoGenerator indexGenerator)
        {
            this.indexGenerator = indexGenerator;
            useCustomizationData = bool.Parse(ConfigurationManager.AppSettings["UseCustomizationData"] ?? Environment.GetEnvironmentVariable("UseCustomizationData") ?? "false");
            _s3BucketName = ConfigurationManager.AppSettings["AWS:S3:DataBucket"];
            _s3KeyPrefix = ConfigurationManager.AppSettings["AWS:S3:KeyPrefix"] ?? "setup/";
            if (!string.IsNullOrEmpty(_s3BucketName))
            {
                _s3Client = new AmazonS3Client();
            }
        }

        protected override void Seed(CatalogDBContext context)
        {
            ExecuteScript(context, CatalogItemHiLoSequenceScript);
            ExecuteScript(context, CatalogBrandHiLoSequenceScript);
            ExecuteScript(context, CatalogTypeHiLoSequenceScript);

            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
            AddCatalogItemPictures();
        }

        private void AddCatalogTypes(CatalogDBContext context)
        {
            var preconfiguredTypes = useCustomizationData
                ? GetCatalogTypesFromFile()
                : PreconfiguredData.GetPreconfiguredCatalogTypes();

            int sequenceId = GetSequenceIdFromSelectedDBSequence(context, DBCatalogSequenceName);
            foreach (var type in preconfiguredTypes)
            {
                type.Id = sequenceId;
                context.CatalogTypes.Add(type);
                sequenceId++;
            }

            context.SaveChanges();
        }

        private void AddCatalogBrands(CatalogDBContext context)
        {
            var preconfiguredBrands = useCustomizationData
                ? GetCatalogBrandsFromFile()
                : PreconfiguredData.GetPreconfiguredCatalogBrands();

            int sequenceId = GetSequenceIdFromSelectedDBSequence(context, DBBrandSequenceName);
            foreach (var brand in preconfiguredBrands)
            {
                brand.Id = sequenceId;
                context.CatalogBrands.Add(brand);
                sequenceId++;
            }

            context.SaveChanges();
        }

        private void AddCatalogItems(CatalogDBContext context)
        {
            var preconfiguredItems = useCustomizationData
                ? GetCatalogItemsFromFile(context)
                : PreconfiguredData.GetPreconfiguredCatalogItems();

            foreach (var item in preconfiguredItems)
            {
                var sequenceId = indexGenerator.GetNextSequenceValue(context);
                item.Id = sequenceId;
                context.CatalogItems.Add(item);
            }

            context.SaveChanges();
        }

        private string ReadFileContent(string relativePath)
        {
            if (_s3Client != null && !string.IsNullOrEmpty(_s3BucketName))
            {
                try
                {
                    var s3Key = _s3KeyPrefix + relativePath.Replace('\\', '/');
                    var response = _s3Client.GetObjectAsync(_s3BucketName, s3Key).Result;
                    using (var reader = new StreamReader(response.ResponseStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
            }

            return null;
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile()
        {
            string csvContent = ReadFileContent("Setup/CatalogTypes.csv");

            if (string.IsNullOrEmpty(csvContent))
            {
                return PreconfiguredData.GetPreconfiguredCatalogTypes();
            }

            string[] csvheaders;

            string[] requiredHeaders = { "catalogtype" };
            csvheaders = GetHeadersFromContent(csvContent, requiredHeaders);

            return csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Skip(1) // skip header row
                                        .Select(x => CreateCatalogType(x))
                                        .Where(x => x != null);
        }

        static CatalogType CreateCatalogType(string type)
        {
            type = type.Trim('"').Trim();

            if (String.IsNullOrEmpty(type))
            {
                throw new Exception("catalog Type Name is empty");
            }

            return new CatalogType
            {
                Type = type,
            };
        }

        static IEnumerable<CatalogBrand> GetCatalogBrandsFromFile()
        {
            string csvContent = ReadFileContent("Setup/CatalogBrands.csv");

            if (string.IsNullOrEmpty(csvContent))
            {
                return PreconfiguredData.GetPreconfiguredCatalogBrands();
            }

            string[] csvheaders;

            string[] requiredHeaders = { "catalogbrand" };
            csvheaders = GetHeadersFromContent(csvContent, requiredHeaders);

            return csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Skip(1) // skip header row
                                        .Select(x => CreateCatalogBrand(x))
                                        .Where(x => x != null);
        }

        static CatalogBrand CreateCatalogBrand(string brand)
        {
            brand = brand.Trim('"').Trim();

            if (String.IsNullOrEmpty(brand))
            {
                throw new Exception("catalog Brand Name is empty");
            }

            return new CatalogBrand
            {
                Brand = brand,
            };
        }

        static IEnumerable<CatalogItem> GetCatalogItemsFromFile(CatalogDBContext context)
        {
            string csvContent = ReadFileContent("Setup/CatalogItems.csv");

            if (string.IsNullOrEmpty(csvContent))
            {
                return PreconfiguredData.GetPreconfiguredCatalogItems();
            }

            string[] csvheaders;
            string[] requiredHeaders = { "catalogtypename", "catalogbrandname", "description", "name", "price", "pictureFileName" };
            string[] optionalheaders = { "availablestock", "restockthreshold", "maxstockthreshold", "onreorder" };
            csvheaders = GetHeadersFromContent(csvContent, requiredHeaders, optionalheaders);

            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

            return csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1) // skip header row
                        .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                        .Select(column => CreateCatalogItem(column, csvheaders, catalogTypeIdLookup, catalogBrandIdLookup))
                        .Where(x => x != null);
        }

        static CatalogItem CreateCatalogItem(string[] column, string[] headers, Dictionary<String, int> catalogTypeIdLookup, Dictionary<String, int> catalogBrandIdLookup)
        {
            if (column.Count() != headers.Count())
            {
                throw new Exception($"column count '{column.Count()}' not the same as headers count'{headers.Count()}'");
            }

            string catalogTypeName = column[Array.IndexOf(headers, "catalogtypename")].Trim('"').Trim();
            if (!catalogTypeIdLookup.ContainsKey(catalogTypeName))
            {
                throw new Exception($"type={catalogTypeName} does not exist in catalogTypes");
            }

            string catalogBrandName = column[Array.IndexOf(headers, "catalogbrandname")].Trim('"').Trim();
            if (!catalogBrandIdLookup.ContainsKey(catalogBrandName))
            {
                throw new Exception($"type={catalogTypeName} does not exist in catalogTypes");
            }

            string priceString = column[Array.IndexOf(headers, "price")].Trim('"').Trim();
            if (!Decimal.TryParse(priceString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out Decimal price))
            {
                throw new Exception($"price={priceString}is not a valid decimal number");
            }

            var catalogItem = new CatalogItem()
            {
                CatalogTypeId = catalogTypeIdLookup[catalogTypeName],
                CatalogBrandId = catalogBrandIdLookup[catalogBrandName],
                Description = column[Array.IndexOf(headers, "description")].Trim('"').Trim(),
                Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
                Price = price,
                PictureFileName = column[Array.IndexOf(headers, "picturefilename")].Trim('"').Trim(),
            };

            int availableStockIndex = Array.IndexOf(headers, "availablestock");
            if (availableStockIndex != -1)
            {
                string availableStockString = column[availableStockIndex].Trim('"').Trim();
                if (!String.IsNullOrEmpty(availableStockString))
                {
                    if (int.TryParse(availableStockString, out int availableStock))
                    {
                        catalogItem.AvailableStock = availableStock;
                    }
                    else
                    {
                        throw new Exception($"availableStock={availableStockString} is not a valid integer");
                    }
                }
            }

            int restockThresholdIndex = Array.IndexOf(headers, "restockthreshold");
            if (restockThresholdIndex != -1)
            {
                string restockThresholdString = column[restockThresholdIndex].Trim('"').Trim();
                if (!String.IsNullOrEmpty(restockThresholdString))
                {
                    if (int.TryParse(restockThresholdString, out int restockThreshold))
                    {
                        catalogItem.RestockThreshold = restockThreshold;
                    }
                    else
                    {
                        throw new Exception($"restockThreshold={restockThreshold} is not a valid integer");
                    }
                }
            }

            int maxStockThresholdIndex = Array.IndexOf(headers, "maxstockthreshold");
            if (maxStockThresholdIndex != -1)
            {
                string maxStockThresholdString = column[maxStockThresholdIndex].Trim('"').Trim();
                if (!String.IsNullOrEmpty(maxStockThresholdString))
                {
                    if (int.TryParse(maxStockThresholdString, out int maxStockThreshold))
                    {
                        catalogItem.MaxStockThreshold = maxStockThreshold;
                    }
                    else
                    {
                        throw new Exception($"maxStockThreshold={maxStockThreshold} is not a valid integer");
                    }
                }
            }

            int onReorderIndex = Array.IndexOf(headers, "onreorder");
            if (onReorderIndex != -1)
            {
                string onReorderString = column[onReorderIndex].Trim('"').Trim();
                if (!String.IsNullOrEmpty(onReorderString))
                {
                    if (bool.TryParse(onReorderString, out bool onReorder))
                    {
                        catalogItem.OnReorder = onReorder;
                    }
                    else
                    {
                        throw new Exception($"onReorder={onReorderString} is not a valid boolean");
                    }
                }
            }

            return catalogItem;
        }

        static string[] GetHeadersFromContent(string csvContent, string[] requiredHeaders, string[] optionalHeaders = null)
        {
            string[] csvheaders = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).First().ToLowerInvariant().Split(',');

            if (csvheaders.Count() < requiredHeaders.Count())
            {
                throw new Exception($"requiredHeader count '{ requiredHeaders.Count()}' is bigger then csv header count '{csvheaders.Count()}' ");
            }

            if (optionalHeaders != null)
            {
                if (csvheaders.Count() > (requiredHeaders.Count() + optionalHeaders.Count()))
                {
                    throw new Exception($"csv header count '{csvheaders.Count()}'  is larger then required '{requiredHeaders.Count()}' and optional '{optionalHeaders.Count()}' headers count");
                }
            }

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!csvheaders.Contains(requiredHeader.ToLowerInvariant()))
                {
                    throw new Exception($"does not contain required header '{requiredHeader}'");
                }
            }

            return csvheaders;
        }

        private static int GetSequenceIdFromSelectedDBSequence(CatalogDBContext context, string dBSequenceName)
        {
            var rawQuery = context.Database.SqlQuery<Int64>($"SELECT NEXT VALUE FOR {dBSequenceName}");
            var sequenceId = (int)rawQuery.Single();
            return sequenceId;
        }

        private void ExecuteScript(CatalogDBContext context, string scriptFile)
        {
            string scriptContent = ReadFileContent(scriptFile);
            context.Database.ExecuteSqlCommand(scriptContent);
        }

        private void AddCatalogItemPictures()
        {
            if (!useCustomizationData)
            {
                return;
            }

            if (_s3Client != null && !string.IsNullOrEmpty(_s3BucketName))
            {
                var contentRootPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo picturePath = new DirectoryInfo(Path.Combine(contentRootPath, "Pics"));
                if (picturePath.Exists)
                {
                    foreach (FileInfo file in picturePath.GetFiles())
                    {
                        file.Delete();
                    }
                }

                string zipFileCatalogItemPictures = "Setup/CatalogItems.zip";
                var s3Key = _s3KeyPrefix + zipFileCatalogItemPictures.Replace('\\', '/');
                try
                {
                    var response = _s3Client.GetObjectAsync(_s3BucketName, s3Key).Result;
                    using (var memoryStream = new MemoryStream())
                    {
                        response.ResponseStream.CopyTo(memoryStream);
                        var zipBytes = memoryStream.ToArray();
                        using (var zipArchive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read, false))
                        {
                            foreach (var entry in zipArchive.Entries)
                            {
                                var entryPath = Path.Combine(picturePath.ToString(), entry.FullName);
                                entry.ExtractToFile(entryPath, true);
                            }
                        }
                    }
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Picture zip not found in S3, skip
                }
            }
            else
            {
                var contentRootPath = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo picturePath = new DirectoryInfo(Path.Combine(contentRootPath, "Pics"));
                foreach (FileInfo file in picturePath.GetFiles())
                {
                    file.Delete();
                }

                string zipFileCatalogItemPictures = Path.Combine(contentRootPath, "Setup", "CatalogItems.zip");
                ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath.ToString());
            }
        }
    }
}