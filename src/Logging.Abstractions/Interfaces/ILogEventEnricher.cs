using Logging.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Interfaces
{
    /// <summary>
    /// Adds additional metadata to a log event (HTTP context, user info, correlation ID, etc.).
    /// </summary>
    public interface ILogEventEnricher
    {
        LogEvent Enrich(LogEvent logEvent);
    }
}
