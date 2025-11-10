using Application.Interfaces;
using Domain.Helpers;
using Domain.Interfaces;
using Infrastructure.Extensions;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register helpers
            services.AddSingleton<JokesRepositoryHelper>();

            // Register repositories
            services.AddScoped<IJokeRepository, JokeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();


            // Register configured HTTP client
            services.AddExternalApis(configuration);

            return services;
        }
    }
}
