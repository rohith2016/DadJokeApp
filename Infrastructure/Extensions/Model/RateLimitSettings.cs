namespace Infrastructure.Extensions.Model
{
    public class RateLimitSettings
    {
        public int RequestsPerMinute { get; set; }
        public int MaxBurst { get; set; } = 0;
    }
}
