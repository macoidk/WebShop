using Microsoft.AspNetCore.Mvc;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUser(int userId)
        {
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return Ok(orders);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByStatus(string status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync((OrderStatus)Enum.Parse(typeof(OrderStatus), status));
            return Ok(orders);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderDto orderDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var createdOrder = await _orderService.CreateOrderAsync(userId, orderDto);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            await _orderService.UpdateOrderStatusAsync(id, (OrderStatus)Enum.Parse(typeof(OrderStatus), status));
            return NoContent();
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPut("{id}/payment-type")]
        public async Task<IActionResult> UpdateOrderPaymentType(int id, [FromBody] string paymentTypeString)
        {
            if (!Enum.TryParse<PaymentType>(paymentTypeString, true, out var paymentTypeEnum))
            {
                return BadRequest("Не вдалося розпізнати тип оплати.");
            }
            await _orderService.UpdateOrderPaymentTypeAsync(id, paymentTypeEnum);
            return NoContent();
        }
    }
}