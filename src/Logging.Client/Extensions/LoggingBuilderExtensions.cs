using Logging.Abstractions.Interfaces;
using Logging.Client.Configuration;
using Logging.Client.Enrichers;
using Logging.Client.Provider;
using Logging.Client.Queue;
using Logging.Client.Sender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Client.Extensions
{
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Registers the ELK logger provider, background sender and enrichers.
        /// </summary>
        public static ILoggingBuilder AddElkLogging(
            this ILoggingBuilder builder,
            Action<ElkLoggerOptions>? configureOptions = null)
        {
            var services = builder.Services;

            // Bind options from configuration (if available) and/or from delegate.
            services.AddOptions<ElkLoggerOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("ElkLogging").Bind(options);
                    configureOptions?.Invoke(options);
                });

            // Core services
            services.AddSingleton<ILogQueue, LogQueue>();
            services.AddSingleton<ILogCorrelationProvider, DefaultCorrelationProvider>();

            // Enrichers
            services.AddSingleton<ILogEventEnricher, HttpContextLogEventEnricher>();

            // HttpClient for sending logs
            services.AddHttpClient("elk-logging");

            // Background sender
            services.AddHostedService<LogSenderBackgroundService>();

            // Logger provider
            services.AddSingleton<ILoggerProvider, ElkLoggerProvider>();

            return builder;
        }
    }
}
