namespace WebShop.BLL.Exceptions
{
    public class ValidationException : Exception
    {
        internal ValidationException(string message) : base(message) { }
    }
}