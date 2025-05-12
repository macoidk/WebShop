namespace WebShop.BLL.DTOs
{
    public class StatisticsDto
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public Dictionary<string, ProductStatisticsDto> ProductStats { get; set; }
    }
}