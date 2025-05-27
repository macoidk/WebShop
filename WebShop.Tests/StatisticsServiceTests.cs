using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Extras.NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
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
    public class StatisticsServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IStatisticsService _sut;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IProductRepository _productRepositoryMock;
        private readonly IOrderRepository _orderRepositoryMock;

        public StatisticsServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _productRepositoryMock = _fixture.Freeze<IProductRepository>();
            _orderRepositoryMock = _fixture.Freeze<IOrderRepository>();

            _unitOfWorkMock.Products.Returns(_productRepositoryMock);
            _unitOfWorkMock.Orders.Returns(_orderRepositoryMock);
            
            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _sut = _autoSubstitute.Resolve<StatisticsService>();
        }

        [Fact]
        public async Task GetShopStatisticsAsync_ReturnsStatistics()
        {
            var orders = _fixture.Build<Order>()
                .With(o => o.OrderItems, _fixture.CreateMany<OrderItem>(2).ToList())
                .CreateMany(3)
                .ToList();
            
            var products = _fixture.CreateMany<Product>(2).ToList();
            
            _orderRepositoryMock.GetAllAsync().Returns(Task.FromResult(orders.AsEnumerable()));
            _productRepositoryMock.GetAllAsync().Returns(Task.FromResult(products.AsEnumerable()));

            var result = await _sut.GetShopStatisticsAsync();

            result.Should().NotBeNull();
            result.ProductStats.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetProductStatisticsAsync_ValidProductId_ReturnsStatistics()
        {
            var productId = _fixture.Create<int>();
            var product = _fixture.Build<Product>()
                .With(p => p.Id, productId)
                .Create();
                
            var orders = _fixture.CreateMany<Order>(2).ToList();
            
            _productRepositoryMock.GetByIdAsync(productId).Returns(Task.FromResult(product));
            _orderRepositoryMock.GetAllAsync().Returns(Task.FromResult(orders.AsEnumerable()));

            var result = await _sut.GetProductStatisticsAsync(productId);

            result.Should().NotBeNull();
            result.ProductId.Should().Be(productId);
        }

        [Fact]
        public async Task GetProductStatisticsAsync_NonExistingProduct_ThrowsNotFoundException()
        {
            var nonExistingId = _fixture.Create<int>();
            _productRepositoryMock.GetByIdAsync(nonExistingId).Returns(Task.FromResult<Product?>(null));

            Func<Task> act = async () => await _sut.GetProductStatisticsAsync(nonExistingId);
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}