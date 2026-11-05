using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class EditItemModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CatalogItem product;

        public ICatalogService CatalogService { get; set; }

        public EditItemModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        public IEnumerable<CatalogBrand> GetBrands()
        {
            return CatalogService.GetCatalogBrands();
        }

        public IEnumerable<CatalogType> GetTypes()
        {
            return CatalogService.GetCatalogTypes();
        }

        public void OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Edit.aspx?id={id}");
            product = CatalogService.FindCatalogItem(id);
        }

        public IActionResult OnPost(int id, string Name, string Description, int BrandDropDownList, int TypeDropDownList, decimal Price, string PictureFileName, int Stock, int Restock, int Maxstock)
        {
            var catalogItem = new CatalogItem
            {
                Id = id,
                Name = Name,
                Description = Description,
                CatalogBrandId = BrandDropDownList,
                CatalogTypeId = TypeDropDownList,
                Price = Price,
                PictureFileName = PictureFileName,
                AvailableStock = Stock,
                RestockThreshold = Restock,
                MaxStockThreshold = Maxstock
            };
            CatalogService.UpdateCatalogItem(catalogItem);

            return RedirectToPage("/Index");
        }
    }
}
