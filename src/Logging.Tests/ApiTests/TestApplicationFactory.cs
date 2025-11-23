using Logging.Abstractions.Interfaces;
using Logging.IngestionApi.Infrastructure.Elastic;
using Logging.IngestionApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ApiKeySettings:ApiKey"] = "test-key",
                ["Elasticsearch:Url"] = "http://localhost:9999",
                ["Elasticsearch:NumberOfShards"] = "1",
                ["Elasticsearch:NumberOfReplicas"] = "0"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ElasticIndexBootstrapper>();
            services.RemoveAll<ILogWriter>();
            services.AddSingleton<ILogWriter, FakeLogWriter>();
        });
    }
}
