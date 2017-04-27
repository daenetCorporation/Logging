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
        LogLevel MinLevel  { get; }

        bool IncludeScopes { get; }

        /// <summary>
        /// Connection string to EventHub
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the minimum backoff interval.
        /// </summary>
        int RetryMinBackoffTimeInSec { get; }

        /// <summary>
        /// Gets  the maximum backoff interval.
        /// </summary>
        int RetryMaxBackoffTimeInSec { get; }

        /// <summary>
        /// Gets or sets the maximum number of allowed retries.
        /// </summary>
        int MaxRetryCount { get; }
        string CategoryName { get; set; }

        bool TryGetSwitch(string name, out LogLevel level);
    }
}
