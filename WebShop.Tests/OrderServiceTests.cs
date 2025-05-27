using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions; 
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
    public class OrderServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IOrderService _sut;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IOrderRepository _orderRepositoryMock;
        private readonly IUserRepository _userRepositoryMock;
        private readonly IMapper _mapperMock; 

        public OrderServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _fixture.Customize<OrderDto>(composer =>
                composer.With(dto => dto.Phone, new Random().Next(100000000, 999999999).ToString() + new Random().Next(0,9).ToString() )
            );
             _fixture.Customize<UserDto>(composer =>
                composer.With(dto => dto.Phone, new Random().Next(100000000, 999999999).ToString() + new Random().Next(0,9).ToString() )
            );

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _orderRepositoryMock = _fixture.Freeze<IOrderRepository>();
            _userRepositoryMock = _fixture.Freeze<IUserRepository>();
            _mapperMock = _fixture.Freeze<IMapper>(); 

            _unitOfWorkMock.Orders.Returns(_orderRepositoryMock);
            _unitOfWorkMock.Users.Returns(_userRepositoryMock); 
            
            _autoSubstitute = new AutoSubstitute(); 
            _autoSubstitute.Provide(_unitOfWorkMock); 
            _autoSubstitute.Provide(_mapperMock); 
            _sut = _autoSubstitute.Resolve<OrderService>(); 
        }

        [Fact]
        public async Task GetOrderByIdAsync_ExistingId_ReturnsOrder()
        {
            var orderId = _fixture.Create<int>();
            var orderFromRepo = _fixture.Build<Order>()
                .With(o => o.Id, orderId)
                .Create();
            var expectedOrderDto = _fixture.Build<OrderDto>()
                .With(dto => dto.Id, orderId)
                .Create();

            _orderRepositoryMock.GetByIdAsync(orderId).Returns(Task.FromResult(orderFromRepo));
            _mapperMock.Map<OrderDto>(orderFromRepo).Returns(expectedOrderDto);

            var result = await _sut.GetOrderByIdAsync(orderId);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedOrderDto);
        }

        [Fact]
        public async Task GetOrderByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
            var nonExistingId = _fixture.Create<int>();
            _orderRepositoryMock.GetByIdAsync(nonExistingId).Returns(Task.FromResult<Order?>(null));

            Func<Task> act = async () => await _sut.GetOrderByIdAsync(nonExistingId);
            
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}