using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Logging.EventHub
{
    public class EventHubLoggerSettings : IEventHubLoggerSettings
    {
        public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();

        public string ConnectionString { get; set; }

        public bool IncludeScopes { get; set; }

        public bool IncludeExceptionStackTrace { get; set; }

        public LogLevel MinLevel { get; set; }

        public string CategoryName { get; set; }

        public RetryPolicy RetryPolicy { get ; set; }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            return Switches.TryGetValue(name, out level);
        }

        public Func<LogLevel, EventId, string, Exception, EventData> EventDataFormatter { get; set; }

    }
}
