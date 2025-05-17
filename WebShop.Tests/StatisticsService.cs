using System.Collections.Generic;
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
    public class StatisticsServiceTests : TestBase
    {
        private IStatisticsService _statisticsService;
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
            _statisticsService = Kernel.Get<IStatisticsService>();
        }

        [Test]
        public async Task GetShopStatisticsAsync_ReturnsStatistics()
        {
            var orders = _fixture.Build<Order>()
                .With(o => o.OrderItems, _fixture.CreateMany<OrderItem>(2).ToList()) 
                .CreateMany(3) // Створюємо 3 замовлення
                .ToList();
            var products = _fixture.CreateMany<Product>(2).ToList();
            _unitOfWork.Orders.GetAllAsync().Returns(Task.FromResult(orders.AsEnumerable()));
            _unitOfWork.Products.GetAllAsync().Returns(Task.FromResult(products.AsEnumerable()));

            var result = await _statisticsService.GetShopStatisticsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalOrders, Is.EqualTo(0));
            Assert.That(result.ProductStats.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetProductStatisticsAsync_ValidProductId_ReturnsStatistics()
        {
            var product = _fixture.Create<Product>();
            var orders = _fixture.CreateMany<Order>(2).ToList();
            _unitOfWork.Products.GetByIdAsync(product.Id).Returns(Task.FromResult(product));
            _unitOfWork.Orders.GetAllAsync().Returns(Task.FromResult(orders.AsEnumerable()));

            var result = await _statisticsService.GetProductStatisticsAsync(product.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductId, Is.EqualTo(product.Id));
        }

        [Test]
        public void GetProductStatisticsAsync_NonExistingProduct_ThrowsNotFoundException()
        {
            _unitOfWork.Products.GetByIdAsync(999).Returns(Task.FromResult<Product>(null));

            Assert.ThrowsAsync<NotFoundException>(() => _statisticsService.GetProductStatisticsAsync(999));
        }
        
        [TearDown]
        public new void TearDown()
        {
            base.TearDown();
            _unitOfWork?.Dispose();
        }
        
    }
}