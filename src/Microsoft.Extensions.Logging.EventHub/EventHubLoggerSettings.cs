using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Logging.EventHub
{
    public class EventHubLoggerSettings : IEventHubLoggerSettings
    {
        public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();

        public string ConnectionString { get; set; }

        public bool IncludeScopes { get; set; }

        public LogLevel MinLevel { get; set; }

        public int RetryMinBackoffTimeInSec { get; set; }

        public int RetryMaxBackoffTimeInSec { get; set; }

        public int MaxRetryCount { get; set; }

        public string CategoryName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            return Switches.TryGetValue(name, out level);
        }
    }
}
