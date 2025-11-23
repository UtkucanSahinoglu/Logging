using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Logging.IngestionApi.Configuration;

namespace Logging.IngestionApi.Infrastructure.Elastic;

public static class ElasticClientFactory
{
    public static ElasticsearchClient Create(ElasticsearchSettings settings)
    {
        var uri = new Uri(settings.Url);

        var config = new ElasticsearchClientSettings(uri)
            .DefaultIndex("logs-bootstrap")       // pek kullanılmayacak ama dursun
            .ThrowExceptions(false)
            .EnableDebugMode();

        // Eğer ileride security açarsan:
        if (!string.IsNullOrWhiteSpace(settings.Username))
        {
            config = config.Authentication(new BasicAuthentication(settings.Username, settings.Password));
        }

        return new ElasticsearchClient(config);
    }
}
