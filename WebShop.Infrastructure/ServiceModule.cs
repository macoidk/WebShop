using AutoMapper;
using Ninject.Modules;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.Interfaces;
using WebShop.BLL.Services;
using WebShop.BLL.Mapping;
using WebShop.DAL;

namespace WebShop.Infrastructure
{
    public class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IUnitOfWork>().To<UnitOfWork>();
            Bind<IProductService>().To<ProductService>();
            Bind<IUserService>().To<UserService>();
            Bind<IOrderService>().To<OrderService>();
            Bind<ICommentService>().To<CommentService>();
            Bind<IRatingService>().To<RatingService>();
            Bind<IStatisticsService>().To<StatisticsService>();
            
            Bind<IMapper>().ToMethod(ctx =>
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<AutoMapperProfile>();
                });
                return config.CreateMapper();
            }).InSingletonScope();
        }
    }
}