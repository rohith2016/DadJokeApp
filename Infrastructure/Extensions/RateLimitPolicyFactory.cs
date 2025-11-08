using Infrastructure.Extensions.Model;
using Polly;

namespace Infrastructure.Extensions
{
    public static class RateLimitPolicyFactory
    {
        public static IAsyncPolicy<HttpResponseMessage> Create(RateLimitSettings settings)
        {
            return Policy.RateLimitAsync<HttpResponseMessage>(
                settings.RequestsPerMinute,
                TimeSpan.FromMinutes(1),
                settings.MaxBurst
            );
        }
    }
}
