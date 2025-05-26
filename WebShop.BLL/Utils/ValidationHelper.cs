using System.Text.RegularExpressions;
using WebShop.BLL.DTOs;
using WebShop.BLL.Exceptions;

namespace WebShop.BLL.Utils
{
    internal static class ValidationHelper
    {
        internal static void ValidateProduct(ProductDto productDto)
        {
            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new ValidationException("Потрібно вказати назву товару.");
            if (productDto.Price <= 0)
                throw new ValidationException("Ціна товару повинна бути більшою за 0.");
            if (productDto.Stock < 0)
                throw new ValidationException("Кількість товару не може бути від'ємною.");
        }

        internal static void ValidateUser(UserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username))
                throw new ValidationException("Потрібно вказати ім'я користувача.");
            if (string.IsNullOrWhiteSpace(userDto.Email) || !userDto.Email.Contains("@"))
                throw new ValidationException("Потрібно вказати валідний email.");
            if (!string.IsNullOrWhiteSpace(userDto.Phone) && !Regex.IsMatch(userDto.Phone, @"^[0-9]+$"))
                throw new ValidationException("Номер телефону повинен містити тільки цифри.");
        }

        internal static void ValidateOrder(OrderDto orderDto)
        {
            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                throw new ValidationException("Замовлення повинно містити принаймні один товар.");
            if (string.IsNullOrWhiteSpace(orderDto.DeliveryAddress))
                throw new ValidationException("Потрібно вказати адресу доставки.");
            if (!string.IsNullOrWhiteSpace(orderDto.Phone) && !Regex.IsMatch(orderDto.Phone, @"^[0-9]+$"))
                throw new ValidationException("Номер телефону повинен містити тільки цифри.");
        }
    }
}