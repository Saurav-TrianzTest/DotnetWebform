using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages
{
    public class AboutModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void OnGet()
        {
            _log.Info("Now loading... /About.aspx");
        }
    }
}
