using Logging.Abstractions.Interfaces;
using Logging.IngestionApi;
using Logging.IngestionApi.Infrastructure.Elastic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Logging.Tests.ApiTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ILogWriter> WriterMock { get; } = new();
    public Mock<ILogValidationService> ValidatorMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(WebHostDefaults.EnvironmentKey, "Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey:HeaderName"] = "X-API-KEY",
                ["ApiKey:Value"] = "test-key",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ElasticIndexBootstrapper>();
            services.RemoveAll<ILogWriter>();
            services.RemoveAll<ILogValidationService>();

            services.AddSingleton<ILogWriter>(WriterMock.Object);
            services.AddSingleton<ILogValidationService>(ValidatorMock.Object);
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("X-API-KEY", "test-key");
    }
}
