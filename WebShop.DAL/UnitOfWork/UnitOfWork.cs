using System.Threading.Tasks;
using WebShop.Abstractions.Repositories;
using WebShop.Abstractions.UnitOfWork;
using WebShop.DAL.Repositories;

namespace WebShop.DAL
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly WebShopDbContext _context;
        private readonly string _blobConnectionString;
        private IProductRepository _productRepository;
        private IUserRepository _userRepository;
        private IOrderRepository _orderRepository;
        private ICommentRepository _commentRepository;
        private IRatingRepository _ratingRepository;

        public UnitOfWork(WebShopDbContext context, string blobConnectionString)
        {
            _context = context;
            _blobConnectionString = blobConnectionString;
        }

        public IProductRepository Products
        {
            get { return _productRepository ??= new ProductRepository(_context, _blobConnectionString); }
        }

        public IUserRepository Users
        {
            get { return _userRepository ??= new UserRepository(_context); }
        }

        public IOrderRepository Orders
        {
            get { return _orderRepository ??= new OrderRepository(_context); }
        }

        public ICommentRepository Comments
        {
            get { return _commentRepository ??= new CommentRepository(_context); }
        }

        public IRatingRepository Ratings
        {
            get { return _ratingRepository ??= new RatingRepository(_context); }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}