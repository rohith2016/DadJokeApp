using Domain.Helpers;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Extensions;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register helpers
            services.AddSingleton<DadJokesDBHelper>();

            // Register repositories
            services.AddScoped<IJokeRepository, JokeRepository>();

            // Register domain services
            services.AddScoped<IJokeClassifier, JokeClassifier>();
            services.AddScoped<IJokeHighlighter, JokeHighlighter>();

            // Register configured HTTP client
            services.AddExternalApis(configuration);

            return services;
        }
    }
}
