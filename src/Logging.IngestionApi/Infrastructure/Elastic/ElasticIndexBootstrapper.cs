using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Logging.Abstractions.Constants;
using Logging.IngestionApi.Configuration;
using Microsoft.Extensions.Logging;

namespace Logging.IngestionApi.Infrastructure.Elastic;

public sealed class ElasticIndexBootstrapper
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchSettings _settings;
    private readonly ILogger<ElasticIndexBootstrapper> _logger;

    public ElasticIndexBootstrapper(
        ElasticsearchClient client,
        ElasticsearchSettings settings,
        ILogger<ElasticIndexBootstrapper> logger)
    {
        _client = client;
        _settings = settings;
        _logger = logger;
    }

    public async Task BootstrapAsync(CancellationToken ct = default)
    {
        await EnsureIlmPolicyAsync(ct);
        await EnsureIndexTemplateAsync(ct);
    }

    private async Task EnsureIlmPolicyAsync(CancellationToken ct)
    {
        var policyName = ElasticIndexNames.LogsIlmPolicyName;

        var policyJson = """
        {
          "policy": {
            "phases": {
              "hot": {
                "min_age": "0d",
                "actions": {
                  "rollover": {
                    "max_age": "7d",
                    "max_size": "50gb"
                  }
                }
              },
              "warm": {
                "min_age": "7d",
                "actions": {
                  "allocate": {
                    "number_of_replicas": 1
                  }
                }
              },
              "cold": {
                "min_age": "30d",
                "actions": {
                  "freeze": {}
                }
              },
              "delete": {
                "min_age": "180d",
                "actions": {
                  "delete": {}
                }
              }
            }
          }
        }
        """;

        var resp = await _client.Transport.PutAsync<StringResponse>(
            $"/_ilm/policy/{policyName}",
            PostData.String(policyJson),
            ct
        );

        if (!resp.ApiCallDetails.HasSuccessfulStatusCode)
        {
            _logger.LogError("Failed to apply ILM policy: {Error}", resp.Body);
        }
    }

    private async Task EnsureIndexTemplateAsync(CancellationToken ct)
    {
        var templateName = ElasticIndexNames.LogsIndexTemplateName;

        var json = """
            {
              "index_patterns": ["logs-*"],
              "template": {
                "settings": {
                  "number_of_shards": 1,
                  "number_of_replicas": 0,
                  "index.lifecycle.name": "logs-ilm"
                },
                "mappings": {
                  "properties": {
                    "@timestamp": { "type": "date" },
                    "message": { "type": "text" },
                    "logLevel": { "type": "keyword" },
                    "serviceName": { "type": "keyword" },
                    "serviceEnvironment": { "type": "keyword" },
                    "serviceVersion": { "type": "keyword" },
                    "traceId": { "type": "keyword" },
                    "spanId": { "type": "keyword" },
                    "userId": { "type": "keyword" },
                    "tenantId": { "type": "keyword" }
                  }
                }
              }
            }
            """;

        var resp = await _client.Transport.PutAsync<StringResponse>(
            $"/_index_template/{templateName}",
            PostData.String(json),
            ct
        );

        if (!resp.ApiCallDetails.HasSuccessfulStatusCode)
            _logger.LogError("Template failed: {Error}", resp.Body);
    }

}
