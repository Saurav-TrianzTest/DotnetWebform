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
    public class EditModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ICatalogService CatalogService { get; set; }

        public EditModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        [BindProperty]
        public CatalogItem CatalogItem { get; set; }

        public IEnumerable<SelectListItem> Brands { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }

        public void OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Edit?id={id}");
            CatalogItem = CatalogService.FindCatalogItem(id);
            LoadDropdowns();
        }

        public IActionResult OnPost(int id)
        {
            _log.Info($"Now loading... /Catalog/Edit?id={id}");
            LoadDropdowns();

            if (ModelState.IsValid)
            {
                CatalogItem.Id = id;
                CatalogService.UpdateCatalogItem(CatalogItem);
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
