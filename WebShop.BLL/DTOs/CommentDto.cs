namespace WebShop.BLL.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}