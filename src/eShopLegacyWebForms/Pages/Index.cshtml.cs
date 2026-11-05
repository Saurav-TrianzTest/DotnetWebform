using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace eShopLegacyWebForms.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        public ICatalogService CatalogService { get; set; }

        public PaginatedItemsViewModel<CatalogItem> Model { get; set; }

        public IndexModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        public void OnGet(int? size, int? index)
        {
            if (size.HasValue && index.HasValue)
            {
                Model = CatalogService.GetCatalogItemsPaginated(size.Value, index.Value);
                _log.Info($"Now loading... /Default.aspx?size={size}&index={index}");
            }
            else
            {
                Model = CatalogService.GetCatalogItemsPaginated(DefaultPageSize, DefaultPageIndex);
                _log.Info($"Now loading... /Default.aspx?size={DefaultPageSize}&index={DefaultPageIndex}");
            }
        }
    }
}
