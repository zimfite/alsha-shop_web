using Dapper;
using System.Data;
using Npgsql;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class FavouritesService
    {
        private readonly string _connectionString;
        private readonly ILogger<FavouritesService> _logger;

        public FavouritesService(IConfiguration configuration, ILogger<FavouritesService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<FavouriteProduct>> GetUserFavoritesAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var sql = @"
                    SELECT 
                        p.id as Id,
                        p.product_name as ProductName,
                        p.price as Price,
                        p.id_subcategory as CategoryId,
                        p.id_seller as SellerId,
                        pi.path as ImageUrl,
                        f.id as FavouriteId,
                        f.created_at as AddedToFavouritesDate,
                        s.name as SellerName,
                        COALESCE(AVG(rp.score), 0.0) as AverageRating,
                        COUNT(rp.id) as ReviewCount
                    FROM public.favorites f
                    INNER JOIN public.products p ON f.id_product = p.id
                    LEFT JOIN public.products_img pi ON p.id = pi.id_product AND pi.is_primary = true
                    LEFT JOIN public.reviews_product rp ON p.id = rp.id_product
                    LEFT JOIN public.sellers s ON p.id_seller = s.id
                    WHERE f.id_user = @UserId
                    GROUP BY 
                        p.id, p.product_name, p.price, p.id_subcategory, p.id_seller, 
                        pi.path, f.id, f.created_at, s.name
                    ORDER BY f.created_at DESC";

                var favorites = await connection.QueryAsync<FavouriteProduct>(sql, new { UserId = userId });

                _logger.LogInformation($"Загружено {favorites.Count()} избранных товаров для пользователя {userId}");

                return favorites.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении избранных товаров для пользователя {userId}");
                return new List<FavouriteProduct>();
            }
        }

        public async Task<bool> AddToFavoritesAsync(int userId, int productId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var checkSql = "SELECT COUNT(*) FROM public.favorites WHERE id_user = @UserId AND id_product = @ProductId";
                var exists = await connection.ExecuteScalarAsync<int>(checkSql, new
                {
                    UserId = userId,
                    ProductId = productId
                });

                if (exists > 0)
                {
                    _logger.LogInformation($"Товар {productId} уже в избранном у пользователя {userId}");
                    return true;
                }

                var sql = @"
                    INSERT INTO public.favorites (id_user, id_product, created_at, updated_at)
                    VALUES (@UserId, @ProductId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

                var result = await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    ProductId = productId
                });

                _logger.LogInformation($"Товар {productId} добавлен в избранное пользователя {userId}");

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при добавлении товара {productId} в избранное пользователя {userId}");
                return false;
            }
        }

        public async Task<bool> RemoveFromFavoritesAsync(int userId, int productId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var sql = "DELETE FROM public.favorites WHERE id_user = @UserId AND id_product = @ProductId";

                var result = await connection.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    ProductId = productId
                });

                _logger.LogInformation($"Товар {productId} удален из избранного пользователя {userId}");

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> IsProductInFavoritesAsync(int userId, int productId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var sql = "SELECT COUNT(*) FROM public.favorites WHERE id_user = @UserId AND id_product = @ProductId";

                var count = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    UserId = userId,
                    ProductId = productId
                });

                return count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> GetFavoritesCountAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var sql = "SELECT COUNT(*) FROM public.favorites WHERE id_user = @UserId";

                var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });

                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> ClearAllFavoritesAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var sql = "DELETE FROM public.favorites WHERE id_user = @UserId";

                var result = await connection.ExecuteAsync(sql, new { UserId = userId });

                _logger.LogInformation($"Все избранные товары удалены для пользователя {userId}");

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<(List<FavouriteProduct> Products, int TotalCount)> GetUserFavoritesWithPaginationAsync(
            int userId, int page = 1, int pageSize = 15)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var offset = (page - 1) * pageSize;

                var sql = @"
                    SELECT 
                        p.id as Id,
                        p.product_name as ProductName,
                        p.price as Price,
                        p.id_subcategory as CategoryId,
                        p.id_seller as SellerId,
                        pi.path as ImageUrl,
                        f.id as FavouriteId,
                        f.created_at as AddedToFavouritesDate,
                        s.name as SellerName,
                        COALESCE(AVG(rp.score), 0.0) as AverageRating,
                        COUNT(rp.id) as ReviewCount
                    FROM public.favorites f
                    INNER JOIN public.products p ON f.id_product = p.id
                    LEFT JOIN public.products_img pi ON p.id = pi.id_product AND pi.is_primary = true
                    LEFT JOIN public.reviews_product rp ON p.id = rp.id_product
                    LEFT JOIN public.sellers s ON p.id_seller = s.id
                    WHERE f.id_user = @UserId
                    GROUP BY 
                        p.id, p.product_name, p.price, p.id_subcategory, p.id_seller, 
                        pi.path, f.id, f.created_at, s.name
                    ORDER BY f.created_at DESC
                    LIMIT @PageSize OFFSET @Offset;

                    SELECT COUNT(DISTINCT f.id) 
                    FROM public.favorites f
                    WHERE f.id_user = @UserId";

                using var multi = await connection.QueryMultipleAsync(sql, new
                {
                    UserId = userId,
                    Offset = offset,
                    PageSize = pageSize
                });

                var products = (await multi.ReadAsync<FavouriteProduct>()).ToList();
                var totalCount = await multi.ReadSingleAsync<int>();

                _logger.LogInformation($"Загружено {products.Count} избранных товаров (страница {page}) для пользователя {userId}");

                return (products, totalCount);
            }
            catch (Exception)
            {
                return (new List<FavouriteProduct>(), 0);
            }
        }

        public async Task<List<int>> GetUserFavoriteIdsAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var sql = "SELECT id_product FROM public.favorites WHERE id_user = @UserId";

                var ids = await connection.QueryAsync<int>(sql, new { UserId = userId });

                return ids.ToList();
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }

        public async Task<List<FavouriteProduct>> GetFavoritesByIdsAsync(int userId, List<int> productIds)
        {
            if (!productIds.Any())
                return new List<FavouriteProduct>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var sql = @"
                    SELECT 
                        p.id as Id,
                        p.product_name as ProductName,
                        p.price as Price,
                        p.id_subcategory as CategoryId,
                        p.id_seller as SellerId,
                        pi.path as ImageUrl,
                        f.id as FavouriteId,
                        f.created_at as AddedToFavouritesDate,
                        s.name as SellerName,
                        COALESCE(AVG(rp.score), 0.0) as AverageRating,
                        COUNT(rp.id) as ReviewCount
                    FROM public.favorites f
                    INNER JOIN public.products p ON f.id_product = p.id
                    LEFT JOIN public.products_img pi ON p.id = pi.id_product AND pi.is_primary = true
                    LEFT JOIN public.reviews_product rp ON p.id = rp.id_product
                    LEFT JOIN public.sellers s ON p.id_seller = s.id
                    WHERE f.id_user = @UserId AND f.id_product = ANY(@ProductIds)
                    GROUP BY 
                        p.id, p.product_name, p.price, p.id_subcategory, p.id_seller, 
                        pi.path, f.id, f.created_at, s.name
                    ORDER BY f.created_at DESC";

                var favorites = await connection.QueryAsync<FavouriteProduct>(sql, new
                {
                    UserId = userId,
                    ProductIds = productIds.ToArray()
                });

                return favorites.ToList();
            }
            catch (Exception)
            {
                return new List<FavouriteProduct>();
            }
        }
    }

    public class FavouritesViewModel
    {
        public List<FavouriteProduct> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 15;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public void CalculateTotalPages()
        {
            if (PageSize > 0 && TotalCount > 0)
            {
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            }
            else
            {
                TotalPages = 0;
            }
        }
    }
}