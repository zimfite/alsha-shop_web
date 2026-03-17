using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    public class FavouritesModel : PageModel
    {
        private readonly DatabaseService _dbService;
        private readonly FavouritesService _favouritesService;

        public List<FavouriteProduct> ProductFavorites { get; set; } = new List<FavouriteProduct>();
        public bool IsUserAuthenticated { get; set; }

        public bool HasFavorites => ProductFavorites?.Any() == true;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }

        public FavouritesModel(DatabaseService dbService, FavouritesService favouritesService)
        {
            _dbService = dbService;
            _favouritesService = favouritesService;
        }

        public async Task OnGetAsync(int page = 1)
        {
            IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false;
            CurrentPage = page;

            Console.WriteLine($"Пользователь авторизован?: {IsUserAuthenticated}");

            if (IsUserAuthenticated)
            {
                var userId = GetCurrentUserId();
                Console.WriteLine($"Id пользователя: {userId}");

                if (userId != null)
                {
                    var result = await _favouritesService.GetUserFavoritesWithPaginationAsync(
                        userId.Value, CurrentPage, PageSize);

                    ProductFavorites = result.Products;
                    TotalCount = result.TotalCount;

                    TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
                    if (TotalPages == 0) TotalPages = 1;

                    Console.WriteLine($"Загружено {ProductFavorites.Count} продуктов");
                }
                else
                {
                    Console.WriteLine("Ошибка: пользователь авторизован, но не имеет Id");
                    ProductFavorites = new List<FavouriteProduct>();
                }
            }
            else
            {
                ProductFavorites = new List<FavouriteProduct>();
            }

            Console.WriteLine($"HasFavorites: {HasFavorites}");
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("UserId")?.Value;

            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId != null)
            {
                return sessionUserId;
            }

            return null;
        }

        public async Task<IActionResult> OnPostRemoveFromFavouritesAsync(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Необходимо авторизоваться";
                return RedirectToPage();
            }

            var success = await _favouritesService.RemoveFromFavoritesAsync(userId.Value, productId);

            if (success)
            {
                TempData["SuccessMessage"] = "Товар удален из избранного";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении товара";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddToFavouritesAsync(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Необходимо авторизоваться";
                return RedirectToPage();
            }

            var success = await _favouritesService.AddToFavoritesAsync(userId.Value, productId);

            if (success)
            {
                TempData["SuccessMessage"] = "Товар добавлен в избранное";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при добавлении товара";
            }

            return RedirectToPage();
        }
    }
}