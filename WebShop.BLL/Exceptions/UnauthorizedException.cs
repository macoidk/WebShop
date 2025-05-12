namespace WebShop.BLL.Exceptions
{
    internal class UnauthorizedException : Exception
    {
        internal UnauthorizedException(string message) : base(message) { }
    }
}