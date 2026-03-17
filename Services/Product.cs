namespace WebApplication1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int QuantityWarehouse { get; set; }
        public int IdSubcategory { get; set; }
        public int IdSeller { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}