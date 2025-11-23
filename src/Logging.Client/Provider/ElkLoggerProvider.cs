using Logging.Abstractions.Interfaces;
using Logging.Client.Configuration;
using Logging.Client.Queue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Client.Provider
{
    /// <summary>
    /// ILoggerProvider that creates ElkLogger instances.
    /// </summary>
    internal sealed class ElkLoggerProvider : ILoggerProvider
    {
        private readonly IOptions<ElkLoggerOptions> _options;
        private readonly ILogQueue _queue;
        private readonly IEnumerable<ILogEventEnricher> _enrichers;

        public ElkLoggerProvider(
            IOptions<ElkLoggerOptions> options,
            ILogQueue queue,
            IEnumerable<ILogEventEnricher> enrichers)
        {
            _options = options;
            _queue = queue;
            _enrichers = enrichers;
        }

        public ILogger CreateLogger(string categoryName)
            => new ElkLogger(categoryName, _options, _queue, _enrichers);

        public void Dispose()
        {
            // Nothing to dispose; the queue is managed by DI.
        }
    }
}
