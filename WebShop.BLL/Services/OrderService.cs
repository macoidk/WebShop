using System.Collections;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.DTOs;
using WebShop.BLL.Exceptions;
using WebShop.BLL.Interfaces;
using WebShop.BLL.Utils;
using WebShop.Models;

namespace WebShop.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("Order not found.");
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(DTOs.OrderStatus status)
        {
            var modelStatus = _mapper.Map<Models.OrderStatus>(status);
            var orders = await _unitOfWork.Orders.GetOrdersByStatusAsync(modelStatus);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, OrderDto orderDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");
            ValidationHelper.ValidateOrder(orderDto);
            var order = _mapper.Map<Order>(orderDto);
            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.Status = Models.OrderStatus.Pending;
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            if (!string.IsNullOrEmpty(user.Address))
            {
                order.DeliveryAddress = user.Address;
            }
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveAsync();
            orderDto.Id = order.Id;
            orderDto.PaymentDeeplink = await GeneratePaymentDeeplinkAsync(orderDto);
            return orderDto;
        }

        public async Task UpdateOrderStatusAsync(int orderId, DTOs.OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new NotFoundException("Order not found.");
            order.Status = _mapper.Map<Models.OrderStatus>(status);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveAsync();
        }

        public async Task<string> GeneratePaymentDeeplinkAsync(OrderDto orderDto)
        {
            if (orderDto.PaymentType == DTOs.PaymentType.BankCard)
            {
                return $"https://send.monobank.ua/jar/8gunpF8zYS";
            }
            return string.Empty;
        }
    }
}