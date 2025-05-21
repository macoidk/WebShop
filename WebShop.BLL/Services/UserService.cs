using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.DTOs;
using WebShop.BLL.Exceptions;
using WebShop.BLL.Interfaces;
using WebShop.BLL.Utils;
using WebShop.Models;
using UserRole = WebShop.BLL.DTOs.UserRole;

namespace WebShop.BLL.Services
{

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("Користувач не знайдено.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
                throw new NotFoundException("Користувач не знайдено.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(DTOs.UserRole role)
        {
            var modelRole = _mapper.Map<Models.UserRole>(role);
            var users = await _unitOfWork.Users.GetUsersByRoleAsync(modelRole);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task RegisterUserAsync(UserDto userDto, string password)
        {
            ValidationHelper.ValidateUser(userDto);
            var existingUser = await _unitOfWork.Users.GetByUsernameAsync(userDto.Username);
            if (existingUser != null)
                throw new ValidationException("Ім'я користувача вже існує.");
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = PasswordHasher.HashPassword(password);
            user.Role = Models.UserRole.RegisteredUser;
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task<UserDto> LoginUserAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            
            var hashOfInput = PasswordHasher.HashPassword(password);
            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedException("Неправильні дані для входу.");
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateUserProfileAsync(UserDto userDto)
        {
            ValidationHelper.ValidateUser(userDto);
            var user = await _unitOfWork.Users.GetByIdAsync(userDto.Id);
            if (user == null)
                throw new NotFoundException("Користувач не знайдено.");
            _mapper.Map(userDto, user);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveAsync();
        }
        
        public async Task<UserDto> CreateUserWithRoleAsync(UserDto userDto, string password, UserRole role)
        {
            ValidationHelper.ValidateUser(userDto);
            var existingUser = await _unitOfWork.Users.GetByUsernameAsync(userDto.Username);
            if (existingUser != null)
                throw new ValidationException("Ім'я користувача вже існує.");
    
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = PasswordHasher.HashPassword(password);
            user.Role = (WebShop.Models.UserRole)(int)role;
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<UserDto>(user);
        }
        
    }
}