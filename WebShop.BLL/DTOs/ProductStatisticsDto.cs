namespace WebShop.BLL.DTOs
{
    public class ProductStatisticsDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }
}