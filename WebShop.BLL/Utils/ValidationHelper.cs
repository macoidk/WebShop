using WebShop.BLL.DTOs;
using WebShop.BLL.Exceptions;

namespace WebShop.BLL.Utils
{
    internal static class ValidationHelper
    {
        internal static void ValidateProduct(ProductDto productDto)
        {
            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new ValidationException("Product name is required.");
            if (productDto.Price <= 0)
                throw new ValidationException("Price must be greater than zero.");
            if (productDto.Stock < 0)
                throw new ValidationException("Stock cannot be negative.");
        }

        internal static void ValidateUser(UserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username))
                throw new ValidationException("Username is required.");
            if (string.IsNullOrWhiteSpace(userDto.Email) || !userDto.Email.Contains("@"))
                throw new ValidationException("Valid email is required.");
        }

        internal static void ValidateOrder(OrderDto orderDto)
        {
            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                throw new ValidationException("Order must contain at least one item.");
            if (string.IsNullOrWhiteSpace(orderDto.DeliveryAddress))
                throw new ValidationException("Delivery address is required.");
        }
    }
}