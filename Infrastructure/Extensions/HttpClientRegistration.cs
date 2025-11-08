using Application.Interfaces;
using Infrastructure.Extensions.Model;
using Infrastructure.ExternalService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class HttpClientRegistration
    {
        public static IServiceCollection AddExternalApis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDadJokeApiClient(configuration);
            return services;
        }

        public static IServiceCollection AddDadJokeApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            var rateLimitSettings = configuration.GetSection("DadJokeApi:RateLimiting").Get<RateLimitSettings>()
                ?? new RateLimitSettings();
            var policy = RateLimitPolicyFactory.Create(rateLimitSettings);


            services.AddHttpClient<IJokeApiClient, DadJokeApiClient>(client =>
            {
                var baseUrl = configuration["DadJokeApi:BaseUrl"] ?? "https://icanhazdadjoke.com/";
                var timeout = configuration.GetValue<int>("DadJokeApi:Timeout", 30);

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "DadJokeApp (https://github.com/yourapp)");
            })
            .AddPolicyHandler(policy);

            return services;
        }
    }
}
