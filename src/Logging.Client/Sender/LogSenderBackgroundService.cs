using Logging.Abstractions.Constants;
using Logging.Abstractions.Contracts;
using Logging.Client.Configuration;
using Logging.Client.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Client.Sender
{
    /// <summary>
    /// Background service that drains the log queue and sends batches
    /// to the ingestion API.
    /// </summary>
    internal sealed class LogSenderBackgroundService : BackgroundService
    {
        private readonly ILogQueue _queue;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ElkLoggerOptions _options;
        private readonly ILogger<LogSenderBackgroundService> _logger;

        public LogSenderBackgroundService(
            ILogQueue queue,
            IHttpClientFactory httpClientFactory,
            IOptions<ElkLoggerOptions> options,
            ILogger<LogSenderBackgroundService> logger)
        {
            _queue = queue;
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("ELK logging is disabled via configuration.");
                return;
            }

            var batch = new List<LogEvent>(_options.BatchSize);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Try to build a batch
                    while (batch.Count < _options.BatchSize &&
                           await _queue.Reader.WaitToReadAsync(stoppingToken))
                    {
                        while (_queue.Reader.TryRead(out var item))
                        {
                            batch.Add(item);
                            if (batch.Count >= _options.BatchSize)
                                break;
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await SendBatchAsync(batch, stoppingToken);
                        batch.Clear();
                    }
                    else
                    {
                        await Task.Delay(_options.FlushInterval, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending log batch.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            // Final flush on shutdown
            if (batch.Count > 0)
            {
                try
                {
                    await SendBatchAsync(batch, CancellationToken.None);
                }
                catch
                {
                    // Ignore on shutdown
                }
            }
        }

        private async Task SendBatchAsync(List<LogEvent> events, CancellationToken cancellationToken)
        {
            if (events.Count == 0)
                return;

            var client = _httpClientFactory.CreateClient("elk-logging");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_options.SendTimeout);

            var batch = new LogBatch
            {
                Events = events,
                Source = _options.ApplicationName
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
            {
                Content = JsonContent.Create(batch)
            };

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                request.Headers.TryAddWithoutValidation(HeaderNames.ApiKey, _options.ApiKey);
            }

            var response = await client.SendAsync(request, cts.Token);

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Log ingestion API returned {StatusCode}. Check API key.",
                    response.StatusCode);
            }
            else if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Log ingestion API returned {StatusCode}.", response.StatusCode);
            }
        }
    }
}
