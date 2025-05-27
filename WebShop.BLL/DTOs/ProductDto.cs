namespace WebShop.BLL.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Category { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}