using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Contracts
{
    public sealed class LogBatch
    {
        public List<LogEvent> Events { get; set; } = new();
        public string Source { get; set; } = string.Empty;
    }

}
