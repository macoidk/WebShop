using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WebShop.BLL.DTOs;

[assembly: InternalsVisibleTo("WebShop.Infrastructure")]
[assembly: InternalsVisibleTo("WebShop.WebAPI")]

namespace WebShop.BLL.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<OrderDto> CreateOrderAsync(int userId, OrderDto orderDto);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<string> GeneratePaymentDeeplinkAsync(OrderDto orderDto);
    }
}