using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using WebShop.BLL.Interfaces;
using WebShop.BLL.Mapping;
using WebShop.BLL.Services;

namespace WebShop.Infrastructure
{
    public static class BLLServiceExtensions
    {
        public static IServiceCollection AddWebShopBLL(this IServiceCollection services)
        {
            services.AddSingleton(provider => new MapperConfiguration(cfg => 
            {
                cfg.AddProfile<AutoMapperProfile>();
            }).CreateMapper());

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IStatisticsService, StatisticsService>();

            return services;
        }
    }
}