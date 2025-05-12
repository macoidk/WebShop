using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebShop.Abstractions.UnitOfWork;
using WebShop.DAL;

namespace WebShop.Infrastructure
{
    public static class DALServiceExtensions
    {
        public static IServiceCollection AddWebShopDAL(this IServiceCollection services, string dbConnectionString, string blobConnectionString)
        {
            services.AddDbContext<WebShopDbContext>(options =>
                options.UseSqlServer(dbConnectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    )
                ));

            services.AddScoped<IUnitOfWork>(provider => new UnitOfWork(
                provider.GetRequiredService<WebShopDbContext>(),
                blobConnectionString));

            return services;
        }
    }
}