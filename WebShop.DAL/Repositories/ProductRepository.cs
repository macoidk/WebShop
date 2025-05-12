using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebShop.Abstractions.Repositories;
using WebShop.Models;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WebShop.Infrastructure")]

namespace WebShop.DAL.Repositories
{
    internal class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "product-images";

        public ProductRepository(WebShopDbContext context, string blobConnectionString) 
            : base(context)
        {
            _blobServiceClient = new BlobServiceClient(blobConnectionString);
        }
        
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _dbSet.Where(p => p.Category == category).ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _dbSet.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithFiltersAsync(string category, string searchTerm, string sortBy)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {

                switch (sortBy)
                {
                    case "price_asc":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    case "newest": 
                        query = query.OrderByDescending(p => p.Id);
                        break;
                    default:
                        query = query.OrderBy(p => p.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            return await query.ToListAsync();
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(imageStream, true);
            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task AddProductWithImagesAsync(Product product, List<Stream> imageStreams, List<string> fileNames)
        {
            product.ImageUrls = new List<string>();
            for (int i = 0; i < imageStreams.Count && i < fileNames.Count; i++)
            {
                string imageUrl = await UploadImageAsync(imageStreams[i], fileNames[i]);
                product.ImageUrls.Add(imageUrl);
            }
            await AddAsync(product);
        }
    }
}