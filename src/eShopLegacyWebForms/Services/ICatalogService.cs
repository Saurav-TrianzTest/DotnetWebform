using System.Collections.Generic;
using System.Threading.Tasks;
using eShopLegacyWebForms.Models;
using System;
using eShopLegacyWebForms.ViewModel;

namespace eShopLegacyWebForms.Services
{
    public interface ICatalogService : IDisposable
    {
        CatalogItem FindCatalogItem(int id);
        IEnumerable<CatalogBrand> GetCatalogBrands();
        PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize, int pageIndex);
        IEnumerable<CatalogType> GetCatalogTypes();
        void CreateCatalogItem(CatalogItem catalogItem);
        void UpdateCatalogItem(CatalogItem catalogItem);
        void RemoveCatalogItem(CatalogItem catalogItem);

        Task<PaginatedItemsViewModel<CatalogItem>> GetCatalogItemsPaginatedAsync(int pageSize, int pageIndex);
        Task<CatalogItem> FindCatalogItemAsync(int id);
        Task<IEnumerable<CatalogBrand>> GetCatalogBrandsAsync();
        Task<IEnumerable<CatalogType>> GetCatalogTypesAsync();
        Task CreateCatalogItemAsync(CatalogItem catalogItem);
        Task UpdateCatalogItemAsync(CatalogItem catalogItem);
        Task RemoveCatalogItemAsync(CatalogItem catalogItem);
    }
}