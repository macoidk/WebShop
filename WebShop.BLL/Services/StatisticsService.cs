using WebShop.BLL.Exceptions;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;

namespace WebShop.BLL.Services
{
    
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StatisticsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StatisticsDto> GetShopStatisticsAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllWithItemsAsync();
            var products = await _unitOfWork.Products.GetAllAsync();
            var stats = new StatisticsDto
            {
                TotalSales = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count(),
                ProductStats = new Dictionary<string, ProductStatisticsDto>()
            };
            foreach (var product in products)
            {
                var productOrders = orders.SelectMany(o => o.OrderItems).Where(oi => oi.ProductId == product.Id);
                stats.ProductStats[product.Name] = new ProductStatisticsDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitsSold = productOrders.Sum(oi => oi.Quantity),
                    Revenue = productOrders.Sum(oi => oi.Quantity * oi.UnitPrice)
                };
            }
            return stats;
        }

        public async Task<ProductStatisticsDto> GetProductStatisticsAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException("Product not found.");
            var orders = await _unitOfWork.Orders.GetAllWithItemsAsync();
            var productOrders = orders.SelectMany(o => o.OrderItems).Where(oi => oi.ProductId == productId);
            return new ProductStatisticsDto
            {
                ProductId = productId,
                ProductName = product.Name,
                UnitsSold = productOrders.Sum(oi => oi.Quantity),
                Revenue = productOrders.Sum(oi => oi.Quantity * oi.UnitPrice)
            };
        }
    }
}