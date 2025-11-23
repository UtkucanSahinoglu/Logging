using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Contracts
{
    /// <summary>
    /// Technical environment information about the application instance.
    /// Maps to ECS service.* fields.
    /// </summary>
    public sealed class ServiceContextInfo
    {
        public string? Version { get; set; }
        public string? InstanceId { get; set; }
        public string? Host { get; set; }
        public string? Region { get; set; }
    }
}
