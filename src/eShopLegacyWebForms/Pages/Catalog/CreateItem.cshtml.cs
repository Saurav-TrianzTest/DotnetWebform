using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class CreateItemModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ICatalogService CatalogService { get; set; }

        public CreateItemModel(ICatalogService catalogService)
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

        public void OnGet()
        {
            _log.Info("Now loading... /Catalog/Create.aspx");
        }

        public IActionResult OnPost(string Name, string Description, int Brand, int Type, decimal Price, int Stock, int Restock, int Maxstock)
        {
            _log.Info($"Now loading... /Catalog/Create.aspx");

            var catalogItem = new CatalogItem
            {
                Name = Name,
                Description = Description,
                CatalogBrandId = Brand,
                CatalogTypeId = Type,
                Price = Price,
                AvailableStock = Stock,
                RestockThreshold = Restock,
                MaxStockThreshold = Maxstock
            };

            CatalogService.CreateCatalogItem(catalogItem);

            return RedirectToPage("/Index");
        }
    }
}
