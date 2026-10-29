using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace eShopLegacyWebForms.Pages
{
    public class ContactModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnGet()
        {
            ViewData["Title"] = "Contact";
            _log.Info("Now loading... /Contact");
        }
    }
}
