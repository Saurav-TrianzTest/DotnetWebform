using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DetailsItemModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CatalogItem product;

        public ICatalogService CatalogService { get; set; }

        public DetailsItemModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        public void OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Details.aspx?id={id}");
            product = CatalogService.FindCatalogItem(id);
        }
    }
}
