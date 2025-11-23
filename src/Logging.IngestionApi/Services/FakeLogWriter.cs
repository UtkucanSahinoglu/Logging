using Logging.Abstractions.Contracts;
using Logging.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Logging.IngestionApi.Services
{
    public sealed class FakeLogWriter : ILogWriter
    {
        private readonly ILogger<FakeLogWriter> _logger;

        public FakeLogWriter(ILogger<FakeLogWriter> logger)
        {
            _logger = logger;
        }

        public Task WriteAsync(LogEvent e, CancellationToken ct = default)
        {
            _logger.LogInformation("FAKE LOG: {@Log}", e);
            return Task.CompletedTask;
        }

        public Task WriteBatchAsync(IReadOnlyCollection<LogEvent> events, CancellationToken ct = default)
        {
            foreach (var e in events)
                _logger.LogInformation("FAKE LOG (Batch): {@Log}", e);

            return Task.CompletedTask;
        }
    }
}
