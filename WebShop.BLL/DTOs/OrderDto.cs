namespace WebShop.BLL.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string? DeliveryAddress { get; set; }
        public PaymentType PaymentType { get; set; }
        public string? PaymentDeeplink { get; set; }
    }
}