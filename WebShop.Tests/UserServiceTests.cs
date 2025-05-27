using System;
using System.Collections.Generic;
using System.Linq;
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
using WebShop.BLL.Utils;
using WebShop.Models;
using Xunit;

namespace WebShop.Tests
{
    public class UserServiceTests
    {
        private readonly IFixture _fixture;
        private readonly AutoSubstitute _autoSubstitute;
        private readonly IUserService _sut;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IUserRepository _userRepositoryMock;
        private readonly IMapper _mapperMock;

        public UserServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _fixture.Customize<UserDto>(composer =>
                composer.With(dto => dto.Phone, new Random().Next(100000000, 999999999).ToString() + new Random().Next(0,9).ToString()) // Generates a 10-digit string
            );

            _unitOfWorkMock = _fixture.Freeze<IUnitOfWork>();
            _userRepositoryMock = _fixture.Freeze<IUserRepository>();
            _mapperMock = _fixture.Freeze<IMapper>();

            _unitOfWorkMock.Users.Returns(_userRepositoryMock);
            
            _autoSubstitute = new AutoSubstitute();
            _autoSubstitute.Provide(_unitOfWorkMock);
            _autoSubstitute.Provide(_mapperMock);
            _sut = _autoSubstitute.Resolve<UserService>();
        }

        [Fact]
        public async Task RegisterUserAsync_ValidData_RegistersUser()
        {
            var userDto = _fixture.Build<UserDto>()
                .With(u => u.Email, "test@example.com")
                .With(u => u.Phone, "0123456789")
                .Create();
            var password = "password123";
            
            var userEntity = _fixture.Build<User>()
                .With(u => u.Username, userDto.Username)
                .With(u => u.Email, userDto.Email)
                .Create(); 

            _userRepositoryMock.GetByUsernameAsync(userDto.Username).Returns(Task.FromResult<User?>(null));
            _mapperMock.Map<User>(userDto).Returns(userEntity);
            _userRepositoryMock.AddAsync(Arg.Is<User>(u => u.Username == userEntity.Username && u.Email == userEntity.Email)).Returns(Task.CompletedTask);
            _unitOfWorkMock.SaveAsync().Returns(Task.CompletedTask);

            await _sut.RegisterUserAsync(userDto, password);

            await _userRepositoryMock.Received(1).AddAsync(Arg.Is<User>(u => u.Username == userEntity.Username && u.Email == userEntity.Email));
            await _unitOfWorkMock.Received(1).SaveAsync();
        }

        [Fact]
        public async Task RegisterUserAsync_ExistingUsername_ThrowsValidationException()
        {
            var userDto = _fixture.Create<UserDto>();
            var existingUser = _fixture.Create<User>();
            _userRepositoryMock.GetByUsernameAsync(userDto.Username).Returns(Task.FromResult(existingUser));

            Func<Task> act = async () => await _sut.RegisterUserAsync(userDto, "password123");
            
            await act.Should().ThrowAsync<ValidationException>();
            
            await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>());
            await _unitOfWorkMock.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task LoginUserAsync_ValidCredentials_ReturnsUser()
        {
            var username = "testuser";
            var password = "password123";
            var userFromRepo = _fixture.Build<User>()
                .With(u => u.Username, username)
                .With(u => u.PasswordHash, PasswordHasher.HashPassword(password))
                .Create();
            
            var expectedUserDto = _fixture.Build<UserDto>()
                .With(dto => dto.Username, username)
                .Create();

            _userRepositoryMock.GetByUsernameAsync(username).Returns(Task.FromResult(userFromRepo));
            _mapperMock.Map<UserDto>(userFromRepo).Returns(expectedUserDto);

            var result = await _sut.LoginUserAsync(username, password);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUserDto);
        }

        [Fact]
        public async Task LoginUserAsync_InvalidPassword_ThrowsUnauthorizedException()
        {
            var username = "testuser";
            var password = "password123";
            var userFromRepo = _fixture.Build<User>()
                .With(u => u.Username, username)
                .With(u => u.PasswordHash, "differenthash")
                .Create();
            _userRepositoryMock.GetByUsernameAsync(username).Returns(Task.FromResult(userFromRepo));

            Func<Task> act = async () => await _sut.LoginUserAsync(username, password);
            
            await act.Should().ThrowAsync<UnauthorizedException>();
        }
    }
}