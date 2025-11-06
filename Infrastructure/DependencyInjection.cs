using Domain.Helpers;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.ExternalService;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            //Register helpers
            services.AddSingleton<JokesTableHelper>();

            // Register repositories
            services.AddScoped<IJokeRepository, JokeRepository>();

            // Register domain services
            services.AddScoped<IJokeClassifier, JokeClassifier>();
            services.AddScoped<IJokeHighlighter, JokeHighlighter>();

            // Register HTTP client for API
            services.AddHttpClient<IJokeApiClient, DadJokeApiClient>();

            return services;
        }
    }
}
