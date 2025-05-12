using System.Threading.Tasks;
using WebShop.BLL.DTOs;

namespace WebShop.BLL.Interfaces
{
    public interface IStatisticsService
    {
        Task<StatisticsDto> GetShopStatisticsAsync();
        Task<ProductStatisticsDto> GetProductStatisticsAsync(int productId);
    }
}