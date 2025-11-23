using Logging.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Interfaces
{
    /// <summary>
    /// Contract for writing log events to a persistent storage (Elasticsearch, DB, file, etc.).
    /// Implemented by the ingestion API or background workers.
    /// </summary>
    public interface ILogWriter
    {
        Task WriteAsync(LogEvent logEvent, CancellationToken cancellationToken = default);

        Task WriteBatchAsync(
            IReadOnlyCollection<LogEvent> logEvents,
            CancellationToken cancellationToken = default);
    }
}
