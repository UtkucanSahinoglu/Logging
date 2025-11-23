using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Contracts
{
    /// <summary>
    /// HTTP request/response metadata for web-based applications.
    /// Maps to ECS http.* fields.
    /// </summary>
    public sealed class HttpContextInfo
    {
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public string? Scheme { get; set; }
        public string? Host { get; set; }
        public string? UserAgent { get; set; }
        public string? ClientIp { get; set; }
        public int? StatusCode { get; set; }
    }
}
