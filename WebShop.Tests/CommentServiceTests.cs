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
using WebShop.BLL.Utils;
using WebShop.Models;
using UserRole = WebShop.BLL.DTOs.UserRole;

namespace WebShopBLL.Tests
{
    [TestFixture]
    public class CommentServiceTests : TestBase
    {
        private ICommentService _commentService;
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
            _commentService = Kernel.Get<ICommentService>();
        }

        [Test]
        public async Task AddCommentAsync_ValidComment_AddsComment()
        {
            var commentDto = _fixture.Create<CommentDto>();
            var product = _fixture.Create<Product>();
            var username = "testuser";
            var password = "password123";
            var user = _fixture.Build<User>()
                .With(u => u.Username, username)
                .With(u => u.PasswordHash, PasswordHasher.HashPassword(password))
                .Create();
            _unitOfWork.Products.GetByIdAsync(commentDto.ProductId).Returns(Task.FromResult(product));
            _unitOfWork.Users.GetByIdAsync(commentDto.UserId).Returns(Task.FromResult(user));
            _unitOfWork.Comments.AddAsync(Arg.Any<Comment>()).Returns(Task.CompletedTask);
            _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

            var result = await _commentService.AddCommentAsync(commentDto);

            Assert.That(result, Is.Not.Null);
            await _unitOfWork.Comments.Received(1).AddAsync(Arg.Any<Comment>());
            await _unitOfWork.Received(1).SaveAsync();
        }

        [Test]
        public async Task AddCommentAsync_UnregisteredUser_ThrowsUnauthorizedException()
        {
            var commentDto = _fixture.Create<CommentDto>();
            var product = _fixture.Create<Product>();
            _unitOfWork.Products.GetByIdAsync(commentDto.ProductId).Returns(Task.FromResult(product));

            Assert.ThrowsAsync<UnauthorizedException>(() => _commentService.AddCommentAsync(commentDto));
        }

        [Test]
        public async Task GetCommentsByProductAsync_ValidProductId_ReturnsComments()
        {
            var productId = 1;
            var comments = _fixture.CreateMany<Comment>(3).ToList();
            _unitOfWork.Comments.GetCommentsByProductAsync(productId).Returns(Task.FromResult(comments.AsEnumerable()));

            var result = await _commentService.GetCommentsByProductAsync(productId);

            Assert.That(result.Count(), Is.EqualTo(3));
        }
        
        [TearDown]
        public new void TearDown()
        {
            base.TearDown();
            _unitOfWork?.Dispose();
        }
        
    }
}