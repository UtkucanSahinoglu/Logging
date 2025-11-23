using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Contracts
{
    /// <summary>
    /// Enterprise log event model aligned with Elastic Common Schema (ECS).
    /// All services within the logging platform communicate using this contract.
    /// </summary>
    public sealed class LogEvent
    {
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string Level { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? Exception { get; set; }
        public string Category { get; set; } = default!;

        // Application context
        public string Application { get; set; } = default!;
        public string Environment { get; set; } = default!;

        // Identity / tenancy
        public string? TenantId { get; set; }
        public string? UserId { get; set; }

        // Distributed tracing
        public string? TraceId { get; set; }
        public string? SpanId { get; set; }
        public string? CorrelationId { get; set; }

        // Context containers
        public ServiceContextInfo ServiceContext { get; set; } = new();
        public HttpContextInfo HttpContext { get; set; } = new();

        // Additional metadata
        public Dictionary<string, string>? Labels { get; set; }
    }
}
