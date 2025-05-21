using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Ninject;
using NSubstitute;
using NUnit.Framework;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.DTOs;
using WebShop.BLL.Exceptions;
using WebShop.BLL.Interfaces;
using WebShop.Models;

namespace WebShopBLL.Tests
{
    [TestFixture]
    public class ProductServiceTests : TestBase
    {
        private IProductService _productService;
        private IUnitOfWork _unitOfWork;
        private Fixture _fixture;

        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            Rebind<IUnitOfWork>(_unitOfWork);
            _productService = Kernel.Get<IProductService>();
        }

        [Test]
        public async Task GetProductByIdAsync_ExistingId_ReturnsProduct()
        {
            var product = _fixture.Create<Product>();
            _unitOfWork.Products.GetByIdAsync(product.Id).Returns(Task.FromResult(product));

            var result = await _productService.GetProductByIdAsync(product.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(product.Id));
        }

        [Test]
        public void GetProductByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
            _unitOfWork.Products.GetByIdAsync(999).Returns(Task.FromResult<Product>(null));

            Assert.ThrowsAsync<NotFoundException>(() => _productService.GetProductByIdAsync(999));
        }

        [Test]
        public async Task AddProductAsync_ValidData_AddsProduct()
        {
            var productDto = _fixture.Create<ProductDto>();
            var imageStreams = new List<Stream> { new MemoryStream() };
            var fileNames = new List<string> { "image.jpg" };
            _unitOfWork.Products.AddProductWithImagesAsync(Arg.Any<Product>(), imageStreams, fileNames).Returns(Task.CompletedTask);
            _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

            await _productService.AddProductAsync(productDto, imageStreams, fileNames);

            await _unitOfWork.Products.Received(1).AddProductWithImagesAsync(Arg.Any<Product>(), imageStreams, fileNames);
            await _unitOfWork.Received(1).SaveAsync();
        }

        [Test]
        public void AddProductAsync_InvalidPrice_ThrowsValidationException()
        {
            var productDto = _fixture.Build<ProductDto>().With(p => p.Price, -1).Create();

            Assert.ThrowsAsync<ValidationException>(() => _productService.AddProductAsync(productDto, new List<Stream>(), new List<string>()));
        }
        
        [TearDown]
        public new void TearDown()
        {
            base.TearDown();
            _unitOfWork?.Dispose();
        }
        
    }
}