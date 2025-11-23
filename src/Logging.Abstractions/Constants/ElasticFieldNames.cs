using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Constants
{
    public static class ElasticFieldNames
    {
        public const string Timestamp = "@timestamp";
        public const string Message = "message";
        public const string LogLevel = "log.level";
        public const string Exception = "error.stack_trace";

        public const string ServiceName = "service.name";
        public const string ServiceVersion = "service.version";
        public const string ServiceEnvironment = "service.environment";

        public const string TraceId = "trace.id";
        public const string SpanId = "span.id";

        public const string UserId = "user.id";
        public const string TenantId = "tenant.id";

        public const string HttpRequestMethod = "http.request.method";
        public const string HttpRequestPath = "url.path";
        public const string HttpStatusCode = "http.response.status_code";
        public const string ClientIp = "client.address";
    }
}
