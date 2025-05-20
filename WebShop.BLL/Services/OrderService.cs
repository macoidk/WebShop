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
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            foreach (var dto in orderDtos)
            {
                if (dto.Status == DTOs.OrderStatus.Pending && dto.PaymentType == DTOs.PaymentType.BankCard)
                {
                    dto.PaymentDeeplink = await GeneratePaymentDeeplinkAsync(dto); 
                }
            }
            return orderDtos;
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
                throw new NotFoundException("Користувач не знайдений.");
            ValidationHelper.ValidateOrder(orderDto);
            var order = _mapper.Map<Order>(orderDto);
            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.Status = Models.OrderStatus.Pending;
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            bool userUpdated = false;
            if (string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(orderDto.FirstName))
            {
                user.FirstName = orderDto.FirstName;
                userUpdated = true;
            }
            if (string.IsNullOrWhiteSpace(user.LastName) && !string.IsNullOrWhiteSpace(orderDto.LastName))
            {
                user.LastName = orderDto.LastName;
                userUpdated = true;
            }
            if (string.IsNullOrWhiteSpace(user.Address) && !string.IsNullOrWhiteSpace(orderDto.DeliveryAddress))
            {
                user.Address = orderDto.DeliveryAddress;
                userUpdated = true;
            }
            if (string.IsNullOrWhiteSpace(user.Phone) && !string.IsNullOrWhiteSpace(orderDto.Phone))
            {
                user.Phone = orderDto.Phone;
                userUpdated = true;
            }
            if (userUpdated)
            {
                await _unitOfWork.Users.UpdateAsync(user);
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveAsync();
            orderDto.Id = order.Id;
            orderDto.PaymentDeeplink = await GeneratePaymentDeeplinkAsync(orderDto);
            return orderDto;
        }

        public async Task UpdateOrderStatusAsync(int orderId, DTOs.OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(orderId);
            order.Status = _mapper.Map<Models.OrderStatus>(status);
            if (status == DTOs.OrderStatus.Completed)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock -= item.Quantity;
                        await _unitOfWork.Products.UpdateAsync(item.Product);
                    }
                }
            }

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

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task UpdateOrderPaymentTypeAsync(int orderId, DTOs.PaymentType paymentType)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new NotFoundException("Замовлення не знайдено.");

            order.PaymentType = _mapper.Map<Models.PaymentType>(paymentType);
            
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveAsync();
        }
    }
}