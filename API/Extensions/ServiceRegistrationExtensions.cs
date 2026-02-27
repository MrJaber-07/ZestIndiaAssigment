using API.Filters;
using Application.Abstractions;
using Application.Repositories;
using Application.Services;
using Infrastructure.Persistence.UnitOfWork;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Infrastructure.Services;

namespace API.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddEntityServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<LogActionFilter>();
            services.AddScoped<ValidationFilter>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}