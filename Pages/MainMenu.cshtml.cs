using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Services;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace WebApplication1.Pages
{
    public class MainMenuModel : PageModel
    {
        private readonly DatabaseService _dbService;
        public List<dynamic> ProductCards { get; set; } = new List<dynamic>();
        public MainMenuModel(DatabaseService dbService)
        {
            _dbService = dbService;
        }
        public async Task OnGetAsync()
        {
            await LoadProductsWithDetailsAsync();
        }
        private async Task LoadProductsWithDetailsAsync()
        {
            try
            {
                var sql = @"
            SELECT 
                p.id,
                p.product_name as ProductName,
                p.price as Price,
                p.id_subcategory as CategoryId,
                p.id_seller as SellerId,
                pi.path as ImageUrl,
                COALESCE(AVG(rp.score), 0) as AverageRating,
                COUNT(rp.id) as ReviewCount
            FROM public.products p
            LEFT JOIN public.products_img pi ON p.id = pi.id_product AND pi.is_primary = true
            LEFT JOIN public.reviews_product rp ON p.id = rp.id_product
            GROUP BY p.id, p.product_name, p.price, p.id_subcategory, p.id_seller, pi.path
            ORDER BY p.created_at DESC
            LIMIT 15";

                var result = await _dbService.QueryAsync<dynamic>(sql);
                ProductCards = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}