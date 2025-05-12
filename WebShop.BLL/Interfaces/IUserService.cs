using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.BLL.DTOs;

namespace WebShop.BLL.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(int id);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role);
        Task RegisterUserAsync(UserDto userDto, string password);
        Task<UserDto> LoginUserAsync(string username, string password);
        Task<UserDto> CreateUserWithRoleAsync(UserDto userDto, string password, UserRole role);
        Task UpdateUserProfileAsync(UserDto userDto);
    }
}