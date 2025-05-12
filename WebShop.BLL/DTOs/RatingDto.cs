namespace WebShop.BLL.DTOs
{
    public class RatingDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Value { get; set; }
    }
}