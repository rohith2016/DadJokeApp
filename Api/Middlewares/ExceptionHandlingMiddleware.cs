using Domain.Models.Exceptions;
using Domain.Models.Exceptions.Domain.Models.Exceptions;
using System.Text.Json;

namespace Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (TooManyRequestsException ex)
            {
                _logger.LogWarning(ex, "Rate limit exceeded for Dad Joke API.");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                var result = JsonSerializer.Serialize(new { error = ex.Message });
                await context.Response.WriteAsync(result);
            }
            catch (ApiTimeoutException ex)
            {
                _logger.LogError(ex, "Request to Dad Joke API timed out.");
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { error = ex.Message });
                await context.Response.WriteAsync(result);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }

}
