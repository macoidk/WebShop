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
    public class RatingServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IRatingService _sut;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IProductRepository _productRepositoryMock;
        private readonly IUserRepository _userRepositoryMock;
        private readonly IRatingRepository _ratingRepositoryMock;
        private readonly IMapper _mapperMock;

        public RatingServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _productRepositoryMock = _fixture.Freeze<IProductRepository>();
            _userRepositoryMock = _fixture.Freeze<IUserRepository>();
            _ratingRepositoryMock = _fixture.Freeze<IRatingRepository>();
            _mapperMock = _fixture.Freeze<IMapper>();

            _unitOfWorkMock.Products.Returns(_productRepositoryMock);
            _unitOfWorkMock.Users.Returns(_userRepositoryMock);
            _unitOfWorkMock.Ratings.Returns(_ratingRepositoryMock);
            
            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);
            _sut = _autoSubstitute.Resolve<RatingService>();
        }

        [Fact]
        public async Task AddRatingAsync_ValidRating_AddsRating()
        {
            var userId = _fixture.Create<int>();
            var productId = _fixture.Create<int>();
            
            var ratingDto = _fixture.Build<RatingDto>()
                .With(r => r.UserId, userId)
                .With(r => r.ProductId, productId)
                .With(r => r.Value, 4)
                .Create();
            
            var productFromRepo = _fixture.Build<Product>().With(p => p.Id, productId).Create();
            var userFromRepo = _fixture.Build<User>().With(u => u.Id, userId).Create();
            
            var ratingEntity = _fixture.Build<Rating>()
                .With(r => r.UserId, userId)
                .With(r => r.ProductId, productId)
                .With(r => r.Value, ratingDto.Value)
                .With(r => r.User, userFromRepo) 
                .With(r => r.Product, productFromRepo)
                .Create();
            
            var addedRatingEntity = _fixture.Build<Rating>()
                .With(r => r.Id, _fixture.Create<int>()) 
                .With(r => r.UserId, userId)
                .With(r => r.ProductId, productId)
                .With(r => r.Value, ratingDto.Value)
                .With(r => r.User, userFromRepo)
                .With(r => r.Product, productFromRepo)
                .Create();

            var expectedRatingDto = _fixture.Build<RatingDto>()
                .With(dto => dto.Id, addedRatingEntity.Id)
                .With(dto => dto.UserId, userId)
                .With(dto => dto.ProductId, productId)
                .With(dto => dto.Value, ratingDto.Value)
                .Create();

            _productRepositoryMock.GetByIdAsync(productId).Returns(Task.FromResult(productFromRepo));
            _userRepositoryMock.GetByIdAsync(userId).Returns(Task.FromResult(userFromRepo));
            _mapperMock.Map<Rating>(ratingDto).Returns(ratingEntity);
            _ratingRepositoryMock.AddAsync(ratingEntity).Returns(Task.FromResult(addedRatingEntity)); 
            _unitOfWorkMock.SaveAsync().Returns(Task.CompletedTask);
            _mapperMock.Map<RatingDto>(addedRatingEntity).Returns(expectedRatingDto);

            var result = await _sut.AddRatingAsync(ratingDto);

            expectedRatingDto.Id = result.Id;
            result.Should().BeEquivalentTo(expectedRatingDto);
            await _ratingRepositoryMock.Received(1).AddAsync(ratingEntity);
            await _unitOfWorkMock.Received(1).SaveAsync();
        }

        [Fact]
        public async Task AddRatingAsync_InvalidValue_ThrowsValidationException()
        {
            var ratingDto = _fixture.Build<RatingDto>()
                .With(r => r.Value, 6) 
                .Create();

            Func<Task> act = async () => await _sut.AddRatingAsync(ratingDto);

            await act.Should().ThrowAsync<ValidationException>();
            
            await _ratingRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Rating>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task GetRatingsByProductAsync_ValidProductId_ReturnsRatings()
        {
            var productId = _fixture.Create<int>();
            var ratingsFromRepo = _fixture.CreateMany<Rating>(3).ToList();
            var expectedRatingDtos = _fixture.CreateMany<RatingDto>(3).ToList();
            
            _ratingRepositoryMock.GetRatingsByProductAsync(productId)
                .Returns(Task.FromResult(ratingsFromRepo.AsEnumerable()));
            _mapperMock.Map<IEnumerable<RatingDto>>(ratingsFromRepo).Returns(expectedRatingDtos);

            var result = await _sut.GetRatingsByProductAsync(productId);

            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedRatingDtos);
        }
    }
}