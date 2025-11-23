using Logging.Abstractions.Contracts;
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
    /// ILogger implementation that writes log events to an in-memory queue
    /// which is processed by a background sender.
    /// </summary>
    internal sealed class ElkLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ElkLoggerOptions _options;
        private readonly ILogQueue _queue;
        private readonly IEnumerable<ILogEventEnricher> _enrichers;

        public ElkLogger(
            string categoryName,
            IOptions<ElkLoggerOptions> options,
            ILogQueue queue,
            IEnumerable<ILogEventEnricher> enrichers)
        {
            _categoryName = categoryName;
            _options = options.Value;
            _queue = queue;
            _enrichers = enrichers;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel)
        {
            if (!_options.Enabled)
                return false;

            return logLevel >= _options.MinLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId,
                                TState state, Exception? exception,
                                Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter is null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            var logEvent = new LogEvent
            {
                Timestamp = DateTimeOffset.UtcNow,
                Level = logLevel.ToString(),
                Message = message,
                Exception = exception?.ToString(),
                Category = _categoryName,
                Application = _options.ApplicationName,
                Environment = _options.Environment,
                Labels = new Dictionary<string, string>
                {
                    ["EventId"] = eventId.Id.ToString()
                }
            };

            // Apply enrichers
            foreach (var enricher in _enrichers)
            {
                logEvent = enricher.Enrich(logEvent);
            }

            // Try to enqueue; if the queue is full we silently drop the event.
            _queue.TryEnqueue(logEvent);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
