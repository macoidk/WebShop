using Microsoft.AspNetCore.Mvc;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Text.Json;
using WebShop.BLL.Exceptions;

namespace WebShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(string category)
        {
            var products = await _productService.GetProductsByCategoryAsync(category);
            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string searchTerm)
        {
            var products = await _productService.SearchProductsAsync(searchTerm);
            return Ok(products);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsWithFilters(
            [FromQuery] string? category = null, 
            [FromQuery] string? searchTerm = null, 
            [FromQuery] string? sortBy = null)
        {
            var products = await _productService.GetProductsWithFiltersAsync(category, searchTerm, sortBy);
            return Ok(products);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public async Task<ActionResult> AddProduct([FromForm] string productDtoJson, [FromForm] List<IFormFile> images)
        {
            var productDto = JsonSerializer.Deserialize<ProductDto>(productDtoJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var imageStreams = images?.Select(i => i.OpenReadStream()).ToList() ?? new List<Stream>();
            var fileNames = images?.Select(i => i.FileName).ToList() ?? new List<string>();
            await _productService.AddProductAsync(productDto, imageStreams, fileNames);
            return CreatedAtAction(nameof(GetProductById), new { id = productDto.Id }, productDto);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            if (id != productDto.Id)
                return BadRequest();
            await _productService.UpdateProductAsync(productDto);
            return NoContent();
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}