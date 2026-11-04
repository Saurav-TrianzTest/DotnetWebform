using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using System;
using System.Threading.Tasks;
using System.Web.UI;

namespace eShopLegacyWebForms.Catalog
{
    public partial class Delete : System.Web.UI.Page
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected CatalogItem productToDelete;

        public ICatalogService CatalogService { get; set; }

        protected async void Page_Load(object sender, EventArgs e)
        {
            var productId = Convert.ToInt32(Page.RouteData.Values["id"]);
            _log.Info($"Now loading... /Catalog/Delete.aspx?id={productId}");
            productToDelete = await CatalogService.FindCatalogItemAsync(productId);

            this.DataBind();
        }

        protected async void Delete_Click(object sender, EventArgs e)
        {
            await CatalogService.RemoveCatalogItemAsync(productToDelete);

            Response.Redirect("~");
        }
    }
}