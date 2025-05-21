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
    public class RatingServiceTests : TestBase
    {
        private IRatingService _ratingService;
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
            _ratingService = Kernel.Get<IRatingService>();
        }

        [Test]
        public async Task AddRatingAsync_ValidRating_AddsRating()
        {
            var ratingDto = _fixture.Build<RatingDto>().With(r => r.Value, 4).Create();
            var product = _fixture.Create<Product>();
            var username = "testuser";
            var password = "password123";
            var user = _fixture.Build<User>()
                .With(u => u.Username, username)
                .With(u => u.PasswordHash, PasswordHasher.HashPassword(password))
                .Create();
            _unitOfWork.Products.GetByIdAsync(ratingDto.ProductId).Returns(Task.FromResult(product));
            _unitOfWork.Users.GetByIdAsync(ratingDto.UserId).Returns(Task.FromResult(user));
            _unitOfWork.Ratings.AddAsync(Arg.Any<Rating>()).Returns(Task.CompletedTask);
            _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

            var result = await _ratingService.AddRatingAsync(ratingDto);

            Assert.That(result, Is.Not.Null);
            await _unitOfWork.Ratings.Received(1).AddAsync(Arg.Any<Rating>());
            await _unitOfWork.Received(1).SaveAsync();
        }

        [Test]
        public void AddRatingAsync_InvalidValue_ThrowsValidationException()
        {
            var ratingDto = _fixture.Build<RatingDto>().With(r => r.Value, 6).Create();

            Assert.ThrowsAsync<ValidationException>(() => _ratingService.AddRatingAsync(ratingDto));
        }

        [Test]
        public async Task GetRatingsByProductAsync_ValidProductId_ReturnsRatings()
        {
            var productId = 1;
            var ratings = _fixture.CreateMany<Rating>(3).ToList();
            _unitOfWork.Ratings.GetRatingsByProductAsync(productId).Returns(Task.FromResult(ratings.AsEnumerable()));

            var result = await _ratingService.GetRatingsByProductAsync(productId);

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