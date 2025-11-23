using Elastic.Clients.Elasticsearch;
using Logging.Abstractions.Constants;
using Logging.Abstractions.Contracts;
using Logging.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.IngestionApi.Services;

public sealed class ElasticsearchLogWriter : ILogWriter
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchLogWriter> _logger;

    public ElasticsearchLogWriter(
        ElasticsearchClient client,
        ILogger<ElasticsearchLogWriter> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task WriteAsync(LogEvent logEvent, CancellationToken ct = default)
        => WriteBatchAsync(new[] { logEvent }, ct);

    public async Task WriteBatchAsync(IReadOnlyCollection<LogEvent> events, CancellationToken ct = default)
    {
        if (!events.Any())
            return;

        var indexName = string.Format(ElasticIndexNames.LogsDailyPattern, DateTime.UtcNow);

        var response = await _client.BulkAsync(b =>
        {
            foreach (var e in events)
            {
                b.Index(indexName)
                 .Create(ToDoc(e));
            }
        }, ct);

        if (!response.IsValidResponse || response.Errors)
        {
            _logger.LogWarning(
                "Bulk insert failed for index {Index}. Error={Error}",
                indexName,
                response.ElasticsearchServerError
            );
        }
    }

    private static Dictionary<string, object?> ToDoc(LogEvent e)
        => new()
        {
            [ElasticFieldNames.Timestamp] = e.Timestamp,
            [ElasticFieldNames.Message] = e.Message,
            [ElasticFieldNames.LogLevel] = e.Level,
            [ElasticFieldNames.Exception] = e.Exception,
            [ElasticFieldNames.ServiceName] = e.Application,
            [ElasticFieldNames.ServiceEnvironment] = e.Environment,
            [ElasticFieldNames.TraceId] = e.TraceId,
            [ElasticFieldNames.SpanId] = e.SpanId,
            [ElasticFieldNames.UserId] = e.UserId,
            [ElasticFieldNames.TenantId] = e.TenantId
        };
}
