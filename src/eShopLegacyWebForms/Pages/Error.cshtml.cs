using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages
{
    public class ErrorModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Error";
        }
    }
}
