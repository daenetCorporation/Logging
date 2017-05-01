using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.EventHub
{

    /// <summary>
    /// Defines the configuration for <see cref="EventHubLogger"/>.
    /// </summary>
    public interface IEventHubLoggerSettings
    {
        LogLevel MinLevel { get; }

        bool IncludeScopes { get; }

        /// <summary>
        /// If set on true exception stack trace will be logged in a case of an error.
        /// </summary>
        bool IncludeExceptionStackTrace { get; }

        /// <summary>
        /// Connection string to EventHub
        /// </summary>
        string ConnectionString { get; }       

        string CategoryName { get; set; }

        bool TryGetSwitch(string name, out LogLevel level);

        /// <summary>
        /// Specifies the retry policy to be used in commuication with EventHub.
        /// </summary>
        RetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Defines method delegate, which can be optionally used to format EvetntData
        /// to be sent to EventHub.
        /// </summary>
        Func<LogLevel, EventId, string, Exception, EventData> EventDataFormatter { get; set; }
    }
}
