using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.BLL.DTOs;

namespace WebShop.BLL.Interfaces
{
    public interface IRatingService
    {
        Task<RatingDto> AddRatingAsync(RatingDto ratingDto);
        Task<IEnumerable<RatingDto>> GetRatingsByProductAsync(int productId);
        Task UpdateRatingAsync(int id, RatingDto ratingDto);
        Task DeleteRatingAsync(int id);
    }
}