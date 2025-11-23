using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Interfaces
{
    public interface ILogCorrelationProvider
    {
        string? GetCurrentCorrelationId();
        string CreateCorrelationId();
    }
}
