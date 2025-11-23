using Logging.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Client.Enrichers
{
    /// <summary>
    /// Default correlation ID provider based on Guid.
    /// </summary>
    internal sealed class DefaultCorrelationProvider : ILogCorrelationProvider
    {
        private static readonly AsyncLocal<string?> _current = new();

        public string? GetCurrentCorrelationId() => _current.Value;

        public string CreateCorrelationId()
        {
            var value = Guid.NewGuid().ToString("N");
            _current.Value = value;
            return value;
        }
    }
}
