using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebShop.Abstractions.Repositories;
using WebShop.Models;

namespace WebShop.DAL.Repositories
{
    internal class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(WebShopDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetCommentsByProductAsync(int productId)
        {
            return await _dbSet.Where(c => c.ProductId == productId).ToListAsync();
        }
    }
}