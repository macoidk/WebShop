using Microsoft.AspNetCore.Mvc;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Manager")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("shop")]
        public async Task<ActionResult<StatisticsDto>> GetShopStatistics()
        {
            var stats = await _statisticsService.GetShopStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ProductStatisticsDto>> GetProductStatistics(int productId)
        {
            var stats = await _statisticsService.GetProductStatisticsAsync(productId);
            return Ok(stats);
        }
    }
}