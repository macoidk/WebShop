using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebShop.Abstractions.Repositories;
using WebShop.Models;

namespace WebShop.DAL.Repositories
{
    internal class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(WebShopDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
        {
            return await _dbSet.Where(o => o.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _dbSet.Where(o => o.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAndStatusAsync(int userId, OrderStatus status)
        {
            return await _dbSet.Where(o => o.UserId == userId && o.Status == status).ToListAsync();
        }
        
        public async Task<IEnumerable<Order>> GetAllWithItemsAsync()
        {
            return await _dbSet.Include(o => o.OrderItems).ToListAsync();
        }
        
    }
}