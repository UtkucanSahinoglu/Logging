using Logging.Abstractions.Constants;
using Logging.Abstractions.Contracts;
using Logging.Abstractions.Interfaces;
using Logging.Client.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Client.Enrichers
{
    /// <summary>
    /// Enriches log events with HTTP and identity information from the current HttpContext.
    /// </summary>
    internal sealed class HttpContextLogEventEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ElkLoggerOptions _options;
        private readonly ILogCorrelationProvider _correlationProvider;

        public HttpContextLogEventEnricher(
            IHttpContextAccessor httpContextAccessor,
            IOptions<ElkLoggerOptions> options,
            ILogCorrelationProvider correlationProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _correlationProvider = correlationProvider;
        }

        public LogEvent Enrich(LogEvent logEvent)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
                return logEvent;

            var request = httpContext.Request;
            var response = httpContext.Response;
            var user = httpContext.User;

            // Identity
            logEvent.UserId ??= user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? user.FindFirst("sub")?.Value;

            // Tenant
            if (string.IsNullOrWhiteSpace(logEvent.TenantId) &&
                request.Headers.TryGetValue(HeaderNames.TenantId, out var tenantHeader))
            {
                logEvent.TenantId = tenantHeader.FirstOrDefault();
            }

            // Correlation & tracing
            if (string.IsNullOrWhiteSpace(logEvent.CorrelationId))
            {
                if (request.Headers.TryGetValue(HeaderNames.CorrelationId, out var corrHeader))
                {
                    logEvent.CorrelationId = corrHeader.FirstOrDefault();
                }
                else
                {
                    logEvent.CorrelationId = _correlationProvider.GetCurrentCorrelationId()
                                           ?? _correlationProvider.CreateCorrelationId();
                }
            }

            if (string.IsNullOrWhiteSpace(logEvent.TraceId) &&
                request.Headers.TryGetValue(HeaderNames.TraceId, out var traceHeader))
            {
                logEvent.TraceId = traceHeader.FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(logEvent.SpanId) &&
                request.Headers.TryGetValue(HeaderNames.SpanId, out var spanHeader))
            {
                logEvent.SpanId = spanHeader.FirstOrDefault();
            }

            // HTTP context
            logEvent.HttpContext.RequestPath ??= request.Path.HasValue ? request.Path.Value : null;
            logEvent.HttpContext.RequestMethod ??= request.Method;
            logEvent.HttpContext.Scheme ??= request.Scheme;
            logEvent.HttpContext.Host ??= request.Host.Value;
            logEvent.HttpContext.UserAgent ??= request.Headers.UserAgent.ToString();
            logEvent.HttpContext.ClientIp ??= httpContext.Connection.RemoteIpAddress?.ToString();

            if (!logEvent.HttpContext.StatusCode.HasValue && response is not null)
            {
                logEvent.HttpContext.StatusCode = response.StatusCode;
            }

            // Service context
            logEvent.ServiceContext.Host ??= Environment.MachineName;

            return logEvent;
        }
    }
}
