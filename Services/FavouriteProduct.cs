namespace WebApplication1.Services
{
    public class FavouriteProduct
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int SellerId { get; set; }
        public string? ImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int FavouriteId { get; set; }
        public DateTime AddedToFavouritesDate { get; set; }
        public string? SellerName { get; set; }
        public string FormattedPrice => Price.ToString("N0") + " ₽";
        public string FormattedRating => AverageRating.ToString("0.0");
        public string FormattedAddedDate => AddedToFavouritesDate.ToString("dd.MM.yyyy");
        public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    }
}