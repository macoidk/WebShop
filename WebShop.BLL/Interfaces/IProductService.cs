using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.BLL.DTOs;

namespace WebShop.BLL.Interfaces
{

    public interface IProductService
    {
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<ProductDto>> GetProductsWithFiltersAsync(string category, string searchTerm, string sortBy);
        Task AddProductAsync(ProductDto productDto, List<Stream> imageStreams, List<string> fileNames);
        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(int id);
    }
}