using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Abstractions.Constants
{
    public static class ElasticIndexNames
    {
        public const string LogsDailyPattern = "logs-{0:yyyy.MM.dd}";
        public const string LogsIlmPolicyName = "logs-ilm-policy";
        public const string LogsIndexTemplateName = "logs-template";
    }
}
