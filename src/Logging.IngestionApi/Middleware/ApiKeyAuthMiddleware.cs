using Logging.Abstractions.Constants;
using Logging.IngestionApi.Configuration;
using Microsoft.Extensions.Options;

namespace Logging.IngestionApi.Middleware
{
    public sealed class ApiKeyAuthenticationMiddleware : IMiddleware
    {
        private readonly ApiKeySettings _settings;

        public ApiKeyAuthenticationMiddleware(IOptions<ApiKeySettings> options)
        {
            _settings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path == "/health" || path == "/api/logs/ping" || path == ("/swagger"))
            {
                await next(context);
                return;
            }


            if (!context.Request.Headers.TryGetValue(_settings.HeaderName, out var providedKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized - Missing API Key");
                return;
            }

            if (!string.Equals(providedKey, _settings.Value, StringComparison.Ordinal))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized - Invalid API Key");
                return;
            }

            await next(context);
        }
    }
}
