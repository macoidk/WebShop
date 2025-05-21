namespace WebShop.BLL.Exceptions
{
    public class NotFoundException : Exception
    {
        internal NotFoundException(string message) : base(message) { }
    }
}