namespace Logging.IngestionApi.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
            => app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }
}
