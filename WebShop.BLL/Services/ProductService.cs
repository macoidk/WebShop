namespace WebShop.BLL.Services
{
    using AutoMapper;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using WebShop.Abstractions.UnitOfWork;
    using WebShop.BLL.DTOs;
    using WebShop.BLL.Exceptions;
    using WebShop.BLL.Interfaces;
    using WebShop.BLL.Utils;
    using WebShop.Models;

    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found.");
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _unitOfWork.Products.GetProductsByCategoryAsync(category);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsWithFiltersAsync(string category, string searchTerm, string sortBy)
        {
            var products = await _unitOfWork.Products.GetProductsWithFiltersAsync(category, searchTerm, sortBy);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task AddProductAsync(ProductDto productDto, List<Stream> imageStreams, List<string> fileNames)
        {
            ValidationHelper.ValidateProduct(productDto);
            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Products.AddProductWithImagesAsync(product, imageStreams, fileNames);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            ValidationHelper.ValidateProduct(productDto);
            var product = await _unitOfWork.Products.GetByIdAsync(productDto.Id);
            if (product == null)
                throw new NotFoundException("Product not found.");
            _mapper.Map(productDto, product);
            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found.");
            foreach (var url in product.ImageUrls)
            {
                var fileName = url.Split('/').Last();
                await _unitOfWork.Products.DeleteImageAsync(fileName);
            }
            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}