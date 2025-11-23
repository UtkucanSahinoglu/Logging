using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Contracts.Enums
{
    /// <summary>
    /// Simplified severity levels for external systems.
    /// </summary>
    public enum LogSeverity
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }
}
