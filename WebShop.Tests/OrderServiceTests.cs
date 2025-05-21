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
    public class OrderServiceTests : TestBase
    {
        private IOrderService _orderService;
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
            _orderService = Kernel.Get<IOrderService>();
        }

        [Test]
        public async Task GetOrderByIdAsync_ExistingId_ReturnsOrder()
        {
            var order = _fixture.Create<Order>();
            _unitOfWork.Orders.GetByIdAsync(order.Id).Returns(Task.FromResult(order));

            var result = await _orderService.GetOrderByIdAsync(order.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(order.Id));
        }

        [Test]
        public void GetOrderByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
            _unitOfWork.Orders.GetByIdAsync(999).Returns(Task.FromResult<Order>(null));

            Assert.ThrowsAsync<NotFoundException>(() => _orderService.GetOrderByIdAsync(999));
        }

        [Test]
        public async Task CreateOrderAsync_ValidData_CreatesOrder()
        {
            var userId = 1;
            var orderDto = _fixture.Build<OrderDto>()
                .With(o => o.OrderItems, _fixture.CreateMany<OrderItemDto>(2).ToList())
                .With(o => o.DeliveryAddress, "Address")
                .Create();
            var user = _fixture.Create<User>();
            _unitOfWork.Users.GetByIdAsync(userId).Returns(Task.FromResult(user));
            _unitOfWork.Orders.AddAsync(Arg.Any<Order>()).Returns(Task.CompletedTask);
            _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

            var result = await _orderService.CreateOrderAsync(userId, orderDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.GreaterThan(0));
            await _unitOfWork.Orders.Received(1).AddAsync(Arg.Any<Order>());
            await _unitOfWork.Received(1).SaveAsync();
        }
        
        
        [TearDown]
        public new void TearDown()
        {
            base.TearDown();
            _unitOfWork?.Dispose();
        }
        
    }
}