using Microsoft.Extensions.Logging;

namespace Logging.Client.Configuration
{
    /// <summary>
    /// Configuration options for the ELK logging client.
    /// </summary>
    public sealed class ElkLoggerOptions
    {
        /// <summary>
        /// Absolute URL of the log ingestion API endpoint.
        /// Example: https://logs.mycompany.com/api/logs/batch
        /// </summary>
        public string Endpoint { get; set; } = default!;

        /// <summary>
        /// API key used to authenticate with the ingestion API.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Logical application name, e.g. "OrderService".
        /// </summary>
        public string ApplicationName { get; set; } = default!;

        /// <summary>
        /// Environment name, e.g. "Production", "Staging", "Development".
        /// </summary>
        public string Environment { get; set; } = "Production";

        /// <summary>
        /// Minimum log level to send to the remote logging backend.
        /// </summary>
        public LogLevel MinLogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Maximum number of log events in a single batch payload.
        /// </summary>
        public int BatchSize { get; set; } = 200;

        /// <summary>
        /// Time window after which a partially filled batch is flushed.
        /// </summary>
        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Maximum number of items in the in-memory log queue.
        /// When the queue is full, new events are dropped.
        /// </summary>
        public int MaxQueueSize { get; set; } = 20_000;

        /// <summary>
        /// Timeout for sending a batch to the ingestion API.
        /// </summary>
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Whether remote logging is enabled. If false, no events are sent.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
