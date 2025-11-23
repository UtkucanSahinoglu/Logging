using Logging.Abstractions.Contracts;
using Logging.Client.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Logging.Client.Queue
{
    /// <summary>
    /// Bounded channel-based implementation of ILogQueue.
    /// </summary>
    internal sealed class LogQueue : ILogQueue
    {
        private readonly Channel<LogEvent> _channel;

        public LogQueue(IOptions<ElkLoggerOptions> options)
        {
            if (options.Value.MaxQueueSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.Value.MaxQueueSize));

            var channelOptions = new BoundedChannelOptions(options.Value.MaxQueueSize)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropWrite
            };

            _channel = Channel.CreateBounded<LogEvent>(channelOptions);
        }

        public bool TryEnqueue(LogEvent logEvent)
            => _channel.Writer.TryWrite(logEvent);

        public ChannelReader<LogEvent> Reader => _channel.Reader;
    }
}
