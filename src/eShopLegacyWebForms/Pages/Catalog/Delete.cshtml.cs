using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace eShopLegacyWebForms.Catalog
{
    public class DeleteModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ICatalogService CatalogService { get; set; }

        public DeleteModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        public CatalogItem CatalogItem { get; set; }

        public void OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Delete?id={id}");
            CatalogItem = CatalogService.FindCatalogItem(id);
        }

        public IActionResult OnPost(int id)
        {
            CatalogItem = CatalogService.FindCatalogItem(id);
            CatalogService.RemoveCatalogItem(CatalogItem);
            return RedirectToPage("/Index");
        }
    }
}
