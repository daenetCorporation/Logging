using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace Microsoft.Extensions.Logging.EventHub
{ 
    public class EventHubLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, EventHubLogger> m_Loggers = new ConcurrentDictionary<string, EventHubLogger>();

        private string m_EventHubName;
        private string m_Hostname;
        private string m_SasToken;
        private string m_SubSystem;
        private Func<string, LogLevel, bool> m_Filter;
        private IEventHubLoggerSettings m_Settings;
        private bool m_IncludeScopes;

        public EventHubLoggerProvider(string eventHubName, string serviceBusNamespace, string sasToken, string subSystem, Func<string, LogLevel, bool> filter, bool includeScopes = false)
        {
            m_EventHubName = eventHubName;
            m_Hostname = serviceBusNamespace;
            m_SasToken = sasToken;
            m_SubSystem = subSystem;
            m_Filter = filter;
            m_IncludeScopes = includeScopes;
        }

        public EventHubLoggerProvider(string eventHubName, string hostName, string sasToken, string subSystem, IEventHubLoggerSettings settings, bool includeScopes = false)
        {
            m_EventHubName = eventHubName;
            m_Hostname = hostName;
            m_SasToken = sasToken;
            m_SubSystem = subSystem;
            m_Settings = settings;
            m_IncludeScopes = includeScopes;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return m_Loggers.GetOrAdd(categoryName, createLoggerImplementation);
        }

        private EventHubLogger createLoggerImplementation(string categoryName)
        {
            return new EventHubLogger(categoryName, m_EventHubName, m_Hostname, m_SasToken, m_SubSystem, getFilter(categoryName, m_Settings), m_IncludeScopes);
        }

        private Func<string, LogLevel, bool> getFilter(string name, IEventHubLoggerSettings settings)
        {
            if (m_Filter != null)
            {
                return m_Filter;
            }

            if (settings != null)
            {
                foreach (var prefix in getKeyPrefixes(name))
                {
                    LogLevel level;
                    if (settings.TryGetSwitch(prefix, out level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return (n, l) => false;
        }

        private IEnumerable<string> getKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }

        public void Dispose()
        {
        }
    }
}
