using Logging.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Logging.Client.Queue
{
    /// <summary>
    /// Abstraction for an in-memory log queue used by the logger
    /// and the background sender.
    /// </summary>
    public interface ILogQueue
    {
        bool TryEnqueue(LogEvent logEvent);
        ChannelReader<LogEvent> Reader { get; }
    }
}
