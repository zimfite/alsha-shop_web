using Azure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    public class ProfileModel : PageModel
    {
        public bool IsUserAuthenticated { get; private set; }

        public void OnGet()
        {
            IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!IsUserAuthenticated)
            {
                // Перенаправляем на страницу входа
                Response.Redirect("MainMenu");
            }
        }
    }
}