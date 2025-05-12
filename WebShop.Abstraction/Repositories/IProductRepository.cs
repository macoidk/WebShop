using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.Models;

namespace WebShop.Abstractions.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsWithFiltersAsync(string category, string searchTerm, string sortBy);
        Task<string> UploadImageAsync(Stream imageStream, string fileName);
        Task DeleteImageAsync(string fileName);
        Task AddProductWithImagesAsync(Product product, List<Stream> imageStreams, List<string> fileNames);
    }
}