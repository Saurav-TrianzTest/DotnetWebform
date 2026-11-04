using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.ViewModel;

namespace eShopLegacyWebForms.Services
{
    public class CatalogServiceMock : ICatalogService
    {
        private List<CatalogItem> catalogItems;

        public CatalogServiceMock()
        {
            catalogItems = new List<CatalogItem>(PreconfiguredData.GetPreconfiguredCatalogItems());
        }

        public PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize = 10, int pageIndex = 0)
        {
            var items = ComposeCatalogItems(catalogItems);
            
            var itemsOnPage = items
                .OrderBy(c => c.Id)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToList();

            return new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, items.Count, itemsOnPage);
        }

        public CatalogItem FindCatalogItem(int id)
        {
            return catalogItems.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<CatalogType> GetCatalogTypes()
        {
            return PreconfiguredData.GetPreconfiguredCatalogTypes();
        }

        public IEnumerable<CatalogBrand> GetCatalogBrands()
        {
            return PreconfiguredData.GetPreconfiguredCatalogBrands();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var maxId = catalogItems.Max(i => i.Id);
            catalogItem.Id = ++maxId;
            catalogItems.Add(catalogItem);
        }

        public void UpdateCatalogItem(CatalogItem modifiedItem)
        {
            var originalItem = FindCatalogItem(modifiedItem.Id);
            if (originalItem != null)
            {
                catalogItems[catalogItems.IndexOf(originalItem)] = modifiedItem;
            }
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            catalogItems.Remove(catalogItem);
        }

        public void Dispose()
        {
        }

        public Task<PaginatedItemsViewModel<CatalogItem>> GetCatalogItemsPaginatedAsync(int pageSize = 10, int pageIndex = 0)
        {
            return Task.FromResult(GetCatalogItemsPaginated(pageSize, pageIndex));
        }

        public Task<CatalogItem> FindCatalogItemAsync(int id)
        {
            return Task.FromResult(FindCatalogItem(id));
        }

        public Task<IEnumerable<CatalogBrand>> GetCatalogBrandsAsync()
        {
            return Task.FromResult(GetCatalogBrands());
        }

        public Task<IEnumerable<CatalogType>> GetCatalogTypesAsync()
        {
            return Task.FromResult(GetCatalogTypes());
        }

        public Task CreateCatalogItemAsync(CatalogItem catalogItem)
        {
            CreateCatalogItem(catalogItem);
            return Task.CompletedTask;
        }

        public Task UpdateCatalogItemAsync(CatalogItem catalogItem)
        {
            UpdateCatalogItem(catalogItem);
            return Task.CompletedTask;
        }

        public Task RemoveCatalogItemAsync(CatalogItem catalogItem)
        {
            RemoveCatalogItem(catalogItem);
            return Task.CompletedTask;
        }

        private List<CatalogItem> ComposeCatalogItems(List<CatalogItem> items)
        {
            var catalogTypes = PreconfiguredData.GetPreconfiguredCatalogTypes();
            var catalogBrands = PreconfiguredData.GetPreconfiguredCatalogBrands();
            items.ForEach(i => i.CatalogBrand = catalogBrands.First(b => b.Id == i.CatalogBrandId));
            items.ForEach(i => i.CatalogType = catalogTypes.First(b => b.Id == i.CatalogTypeId));

            return items;
            ;
        }
    }
}