using Application.Cache;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;


namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IJokeService, JokeService>();
            services.AddScoped<IJokeSearchService, JokeSearchService>();
            services.AddScoped<ICacheService, MemoryCacheService>();


            // Register domain services
            services.AddScoped<IJokeClassifier, JokeClassifier>();
            services.AddScoped<IJokeHighlighter, JokeHighlighter>();

            return services;
        }
    }
}
