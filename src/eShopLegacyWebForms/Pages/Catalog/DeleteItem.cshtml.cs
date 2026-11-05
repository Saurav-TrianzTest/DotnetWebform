using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DeleteItemModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CatalogItem productToDelete;

        public ICatalogService CatalogService { get; set; }

        public DeleteItemModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        public void OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Delete.aspx?id={id}");
            productToDelete = CatalogService.FindCatalogItem(id);
        }

        public IActionResult OnPost(int id)
        {
            var productToDelete = CatalogService.FindCatalogItem(id);
            CatalogService.RemoveCatalogItem(productToDelete);

            return RedirectToPage("/Index");
        }
    }
}
