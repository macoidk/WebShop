using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.Models;

namespace WebShop.Abstractions.Repositories
{
    public interface IRatingRepository : IGenericRepository<Rating>
    {
        Task<IEnumerable<Rating>> GetRatingsByProductAsync(int productId);
    }
}