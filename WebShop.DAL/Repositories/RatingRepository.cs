using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebShop.Abstractions.Repositories;
using WebShop.Models;

namespace WebShop.DAL.Repositories
{
    internal class RatingRepository : GenericRepository<Rating>, IRatingRepository
    {
        public RatingRepository(WebShopDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Rating>> GetRatingsByProductAsync(int productId)
        {
            return await _dbSet.Where(r => r.ProductId == productId).ToListAsync();
        }
    }
}