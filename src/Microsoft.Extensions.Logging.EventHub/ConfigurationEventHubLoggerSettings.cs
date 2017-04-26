using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Logging.EventHub
{
    public class ConfigurationEventHubLoggerSettings : IEventHubLoggerSettings
    {
        private readonly IConfiguration m_Configuration;
        private readonly IConfiguration m_EventHubSection;

        public ConfigurationEventHubLoggerSettings(IConfiguration config)
        {
            m_Configuration = config;
            m_EventHubSection = config.GetSection("EventHub");
        }

        public string ConnectionString
        {
            get
            {
                return getEventHubSectionValue(nameof(ConnectionString));
            }
        }

        public bool IncludeScopes
        {
            get
            {
                bool includeScopes;
                var value = m_Configuration["IncludeScopes"];
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
                else if (bool.TryParse(value, out includeScopes))
                {
                    return includeScopes;
                }
                else
                {
                    var message = $"Configuration value '{value}' for setting '{nameof(IncludeScopes)}' is not supported.";
                    throw new InvalidOperationException(message);
                }
            }
        }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            var switches = m_Configuration.GetSection("LogLevel");
            if (switches == null)
            {
                level = LogLevel.None;
                return false;
            }

            var value = switches[name];
            if (string.IsNullOrEmpty(value))
            {
                level = LogLevel.None;
                return false;
            }
            else if (Enum.TryParse<LogLevel>(value, out level))
            {
                return true;
            }
            else
            {
                var message = $"Configuration value '{value}' for category '{name}' is not supported.";
                throw new InvalidOperationException(message);
            }
        }


        #region Private Methods

        private string getEventHubSectionValue(string key)
        {
            var value = m_EventHubSection[key];
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            else
                return value;
        }

        #endregion
    }
}
