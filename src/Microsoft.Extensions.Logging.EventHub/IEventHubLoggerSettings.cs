using Microsoft.Azure.EventHubs;

namespace Microsoft.Extensions.Logging.EventHub
{

    /// <summary>
    /// Defines the configuration for <see cref="EventHubLogger"/>.
    /// </summary>
    public interface IEventHubLoggerSettings
    {
        bool IncludeScopes { get; }

        bool TryGetSwitch(string name, out LogLevel level);

        /// <summary>
        /// Connection string to EventHub
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// If set on true exception stack trace will be logged in a case of an error.
        /// </summary>
        bool IncludeExceptionStackTrace { get; }

        /// <summary>
        /// Specifies the retry policy to be used in commuication with EventHub.
        /// </summary>
        RetryPolicy RetryPolicy { get; set; }
    }
}
