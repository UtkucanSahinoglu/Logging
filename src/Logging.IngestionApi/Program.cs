using Elastic.Clients.Elasticsearch;
using Logging.Abstractions.Interfaces;
using Logging.IngestionApi.Configuration;
using Logging.IngestionApi.Endpoints;
using Logging.IngestionApi.Infrastructure.Elastic;
using Logging.IngestionApi.Middleware;
using Logging.IngestionApi.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

builder.Configuration.AddEnvironmentVariables();
builder.Host.UseEnvironment(builder.Environment.EnvironmentName);

// Settings
services.Configure<ApiKeySettings>(config.GetSection(ApiKeySettings.SectionName));
services.Configure<ElasticsearchSettings>(config.GetSection(ElasticsearchSettings.SectionName));

services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ElasticsearchSettings>>().Value);

// Middleware
services.AddSingleton<ApiKeyAuthenticationMiddleware>();

// Validation
services.AddSingleton<ILogValidationService, LogValidationService>();

// Elasticsearch client
services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = sp.GetRequiredService<ElasticsearchSettings>();
    return ElasticClientFactory.Create(settings);
});

// Bootstrapper
services.AddSingleton<ElasticIndexBootstrapper>();

// Writers
if (builder.Environment.IsDevelopment() && config.GetValue<bool>("UseFakeWriter"))
{
    services.AddSingleton<ILogWriter, FakeLogWriter>();
}
else
{
    services.AddSingleton<ILogWriter, ElasticsearchLogWriter>();
}

// Swagger
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key giriniz: X-API-Key: {key}",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseApiKeyAuthentication();


// Bootstrap index + ILM
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var bootstrapper = scope.ServiceProvider.GetRequiredService<ElasticIndexBootstrapper>();
    await bootstrapper.BootstrapAsync();
}

app.MapLogEndpoints();
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();

public partial class Program { }
