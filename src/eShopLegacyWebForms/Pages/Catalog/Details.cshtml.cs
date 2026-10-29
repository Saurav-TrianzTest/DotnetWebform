using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace eShopLegacyWebForms.Catalog
{
    public class DetailsModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ICatalogService CatalogService { get; set; }

        public DetailsModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        public CatalogItem CatalogItem { get; set; }

        public void OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Details?id={id}");
            CatalogItem = CatalogService.FindCatalogItem(id);
        }
    }
}
