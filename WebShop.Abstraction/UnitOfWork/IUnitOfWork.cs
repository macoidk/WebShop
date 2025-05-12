using System.Threading.Tasks;
using WebShop.Abstractions.Repositories;

namespace WebShop.Abstractions.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IUserRepository Users { get; }
        IOrderRepository Orders { get; }
        ICommentRepository Comments { get; }
        IRatingRepository Ratings { get; }
        Task SaveAsync();
    }
}