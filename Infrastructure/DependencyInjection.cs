using Application.Interfaces;
using Domain.Helpers;
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

            // Register configured HTTP client
            services.AddExternalApis(configuration);

            return services;
        }
    }
}
