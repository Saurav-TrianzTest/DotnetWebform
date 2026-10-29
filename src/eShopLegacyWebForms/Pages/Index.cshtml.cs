using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace eShopLegacyWebForms.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        public ICatalogService CatalogService { get; set; }

        public PaginatedItemsViewModel<CatalogItem> PaginatedItems { get; set; }

        public IndexModel(ICatalogService catalogService)
        {
            CatalogService = catalogService;
        }

        public void OnGet(int? size, int? index)
        {
            int pageSize = size ?? DefaultPageSize;
            int pageIndex = index ?? DefaultPageIndex;

            PaginatedItems = CatalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
            _log.Info($"Now loading... /Index?size={pageSize}&index={pageIndex}");
        }
    }
}
