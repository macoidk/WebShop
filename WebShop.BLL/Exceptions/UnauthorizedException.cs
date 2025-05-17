namespace WebShop.BLL.Exceptions
{
    public class UnauthorizedException : Exception
    {
        internal UnauthorizedException(string message) : base(message) { }
    }
}