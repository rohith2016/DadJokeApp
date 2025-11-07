using Application.Cache;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;


namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IJokeService, JokeService>();
            services.AddScoped<IJokeSearchService, JokeSearchService>();
            services.AddScoped<ICacheService, MemoryCacheService>();

            return services;
        }
    }
}
