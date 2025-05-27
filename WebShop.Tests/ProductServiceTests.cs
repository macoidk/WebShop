using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using WebShop.Abstractions.Repositories;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.DTOs;
using WebShop.BLL.Exceptions;
using WebShop.BLL.Interfaces;
using WebShop.BLL.Services;
using WebShop.Models;
using Xunit;

namespace WebShop.Tests
{
    public class ProductServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IProductService _sut;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IProductRepository _productRepositoryMock;
        private readonly IMapper _mapperMock;

        public ProductServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _productRepositoryMock = _fixture.Freeze<IProductRepository>();
            _mapperMock = _fixture.Freeze<IMapper>();

            _unitOfWorkMock.Products.Returns(_productRepositoryMock);
            
            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);
            _sut = _autoSubstitute.Resolve<ProductService>();
        }

        [Fact]
        public async Task GetProductByIdAsync_ExistingId_ReturnsProduct()
        {
            var productId = _fixture.Create<int>();
            var productFromRepo = _fixture.Build<Product>()
                .With(p => p.Id, productId)
                .Create();
            var expectedProductDto = _fixture.Build<ProductDto>()
                .With(dto => dto.Id, productId)
                .Create();

            _productRepositoryMock.GetByIdAsync(productId).Returns(Task.FromResult(productFromRepo));
            _mapperMock.Map<ProductDto>(productFromRepo).Returns(expectedProductDto);

            var result = await _sut.GetProductByIdAsync(productId);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedProductDto);
        }

        [Fact]
        public async Task GetProductByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
            var nonExistingId = _fixture.Create<int>();
            _productRepositoryMock.GetByIdAsync(nonExistingId).Returns(Task.FromResult<Product?>(null));

            Func<Task> act = async () => await _sut.GetProductByIdAsync(nonExistingId);
            
            act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task AddProductAsync_ValidData_AddsProduct()
        {
            var productDto = _fixture.Create<ProductDto>();
            var productEntity = _fixture.Create<Product>();
            var imageStreams = new List<Stream> { new MemoryStream() };
            var fileNames = new List<string> { "image.jpg" };
            
            _mapperMock.Map<Product>(productDto).Returns(productEntity);
            _productRepositoryMock.AddProductWithImagesAsync(productEntity, imageStreams, fileNames)
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.SaveAsync().Returns(Task.CompletedTask);

            await _sut.AddProductAsync(productDto, imageStreams, fileNames);

            await _productRepositoryMock.Received(1).AddProductWithImagesAsync(productEntity, imageStreams, fileNames);
            await _unitOfWorkMock.Received(1).SaveAsync();
        }

        [Fact]
        public async Task AddProductAsync_InvalidPrice_ThrowsValidationException()
        {
            var productDto = _fixture.Build<ProductDto>()
                .With(p => p.Price, -1)
                .Create();
            var emptyStreams = new List<Stream>();
            var emptyFileNames = new List<string>();

            Func<Task> act = async () => await _sut.AddProductAsync(productDto, emptyStreams, emptyFileNames);
            
            await act.Should().ThrowAsync<ValidationException>();
            
            await _productRepositoryMock.DidNotReceive().AddProductWithImagesAsync(
                Arg.Any<Product>(), Arg.Any<List<Stream>>(), Arg.Any<List<string>>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }
    }
}