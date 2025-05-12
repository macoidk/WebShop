using Microsoft.AspNetCore.Mvc;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RatingDto>> AddRating([FromBody] RatingDto ratingDto)
        {
            var addedRating = await _ratingService.AddRatingAsync(ratingDto);
            return CreatedAtAction(nameof(GetRatingsByProduct), new { productId = addedRating.ProductId }, addedRating);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<RatingDto>>> GetRatingsByProduct(int productId)
        {
            var ratings = await _ratingService.GetRatingsByProductAsync(productId);
            return Ok(ratings);
        }
    }
}