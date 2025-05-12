using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.Models;

namespace WebShop.Abstractions.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    }
}