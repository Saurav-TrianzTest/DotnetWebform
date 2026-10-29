using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Reflection;

namespace eShopLegacyWebForms.Catalog
{
    public class CreateModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ICatalogService CatalogService { get; set; }

        public CreateModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        [BindProperty]
        public CatalogItem CatalogItem { get; set; }

        public IEnumerable<SelectListItem> Brands { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }

        public void OnGet()
        {
            _log.Info($"Now loading... /Catalog/Create");
            LoadDropdowns();
        }

        public IActionResult OnPost()
        {
            _log.Info($"Now loading... /Catalog/Create");
            LoadDropdowns();

            if (ModelState.IsValid)
            {
                CatalogService.CreateCatalogItem(CatalogItem);
                return RedirectToPage("/Index");
            }

            return Page();
        }

        private void LoadDropdowns()
        {
            Brands = GetSelectList(CatalogService.GetCatalogBrands(), "Id", "Brand");
            Types = GetSelectList(CatalogService.GetCatalogTypes(), "Id", "Type");
        }

        private IEnumerable<SelectListItem> GetSelectList(IEnumerable<dynamic> items, string valueField, string textField)
        {
            var list = new List<SelectListItem>();
            foreach (var item in items)
            {
                list.Add(new SelectListItem
                {
                    Value = item.GetType().GetProperty(valueField).GetValue(item).ToString(),
                    Text = item.GetType().GetProperty(textField).GetValue(item).ToString()
                });
            }
            return list;
        }
    }
}
