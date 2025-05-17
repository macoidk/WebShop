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

namespace WebShopBLL.Tests
{
    [TestFixture]
    public class UserServiceTests : TestBase
    {
        private IUserService _userService;
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
            _userService = Kernel.Get<IUserService>();
        }

        [Test]
        public async Task RegisterUserAsync_ValidData_RegistersUser()
        {
            var userDto = _fixture.Build<UserDto>()
                .With(u => u.Email, "test@example.com")
                .Create();
            var password = "password123";
            _unitOfWork.Users.GetByUsernameAsync(userDto.Username).Returns(Task.FromResult<User>(null));
            _unitOfWork.Users.AddAsync(Arg.Any<User>()).Returns(Task.CompletedTask);
            _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

            await _userService.RegisterUserAsync(userDto, password);

            await _unitOfWork.Users.Received(1).AddAsync(Arg.Any<User>());
            await _unitOfWork.Received(1).SaveAsync();
        }

        [Test]
        public void RegisterUserAsync_ExistingUsername_ThrowsValidationException()
        {
            var userDto = _fixture.Create<UserDto>();
            var existingUser = _fixture.Create<User>();
            _unitOfWork.Users.GetByUsernameAsync(userDto.Username).Returns(Task.FromResult(existingUser));

            Assert.ThrowsAsync<ValidationException>(() => _userService.RegisterUserAsync(userDto, "password123"));
        }

        [Test]
        public async Task LoginUserAsync_ValidCredentials_ReturnsUser()
        {
            var username = "testuser";
            var password = "password123";
            var user = _fixture.Build<User>()
                .With(u => u.Username, username)
                .With(u => u.PasswordHash, PasswordHasher.HashPassword(password))
                .Create();
            _unitOfWork.Users.GetByUsernameAsync(username).Returns(Task.FromResult(user));

            var result = await _userService.LoginUserAsync(username, password);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo(username));
        }

        [Test]
        public void LoginUserAsync_InvalidPassword_ThrowsUnauthorizedException()
        {
            var username = "testuser";
            var password = "password123";
            var user = _fixture.Build<User>()
                .With(u => u.Username, username)
                .With(u => u.PasswordHash, "differenthash")
                .Create();
            _unitOfWork.Users.GetByUsernameAsync(username).Returns(Task.FromResult(user));

            Assert.ThrowsAsync<UnauthorizedException>(() => _userService.LoginUserAsync(username, password));
        }
        
        [TearDown]
        public new void TearDown()
        {
            base.TearDown();
            _unitOfWork?.Dispose();
        }
        
    }
}