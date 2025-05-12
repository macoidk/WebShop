namespace WebShop.BLL.Exceptions
{
    internal class ValidationException : Exception
    {
        internal ValidationException(string message) : base(message) { }
    }
}