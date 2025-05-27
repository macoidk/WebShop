using System;
using System.Collections.Generic;
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
    public class CommentServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly ICommentService _sut;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IProductRepository _productRepositoryMock;
        private readonly IUserRepository _userRepositoryMock;
        private readonly ICommentRepository _commentRepositoryMock;
        private readonly IMapper _mapperMock;

        public CommentServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _productRepositoryMock = _fixture.Freeze<IProductRepository>();
            _userRepositoryMock = _fixture.Freeze<IUserRepository>();
            _commentRepositoryMock = _fixture.Freeze<ICommentRepository>();
            _mapperMock = _fixture.Freeze<IMapper>();

            _unitOfWorkMock.Products.Returns(_productRepositoryMock);
            _unitOfWorkMock.Users.Returns(_userRepositoryMock);
            _unitOfWorkMock.Comments.Returns(_commentRepositoryMock);
            
            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);
            _sut = _autoSubstitute.Resolve<CommentService>();
        }

        [Fact]
        public async Task AddCommentAsync_ValidComment_AddsComment()
        {
            var userId = _fixture.Create<int>();
            var product = _fixture.Create<Product>();
            var user = _fixture.Build<User>()
                .With(u => u.Id, userId)
                .With(u => u.Username, "testuser")
                .Create();
            
            var commentDto = _fixture.Build<CommentDto>()
                .With(c => c.ProductId, product.Id)
                .With(c => c.Text, "Test comment")
                .Create();

            var commentEntity = _fixture.Build<Comment>()
                .With(c => c.ProductId, commentDto.ProductId)
                .With(c => c.UserId, userId)
                .With(c => c.Text, commentDto.Text)
                .With(c => c.User, user)
                .With(c => c.Product, product)
                .Create();

            _productRepositoryMock.GetByIdAsync(product.Id).Returns(Task.FromResult(product));
            _userRepositoryMock.GetByIdAsync(userId).Returns(Task.FromResult(user));
            
            _mapperMock.Map<Comment>(commentDto).Returns(commentEntity);
            _commentRepositoryMock.AddAsync(commentEntity).Returns(Task.CompletedTask);
            _unitOfWorkMock.SaveAsync().Returns(Task.CompletedTask);
            _mapperMock.Map<CommentDto>(commentEntity).Returns(commentDto);

            var result = await _sut.AddCommentAsync(commentDto, userId);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(commentDto);
            await _commentRepositoryMock.Received(1).AddAsync(commentEntity);
            await _unitOfWorkMock.Received(1).SaveAsync();
        }

        [Fact]
        public async Task AddCommentAsync_UnregisteredUser_ThrowsUnauthorizedException()
        {
            var userId = _fixture.Create<int>();
            var product = _fixture.Create<Product>();
            var commentDto = _fixture.Build<CommentDto>()
                .With(c => c.ProductId, product.Id)
                .Create();

            _productRepositoryMock.GetByIdAsync(product.Id).Returns(Task.FromResult(product));
            _userRepositoryMock.GetByIdAsync(userId).Returns(Task.FromResult<User?>(null));

            Func<Task> act = async () => await _sut.AddCommentAsync(commentDto, userId);
            await act.Should().ThrowAsync<UnauthorizedException>();
            
            await _commentRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Comment>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }
    }
}