using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Constants
{
    public static class HeaderNames
    {
        public const string ApiKey = "X-Api-Key";
        public const string TenantId = "X-Tenant-Id";
        public const string CorrelationId = "X-Correlation-Id";
        public const string TraceId = "X-Trace-Id";
        public const string SpanId = "X-Span-Id";
    }
}
