using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Microsoft.Extensions.Logging.EventHub
{
    public class EventHubLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, EventHubLogger> m_Loggers = new ConcurrentDictionary<string, EventHubLogger>();

        private Func<string, LogLevel, bool> m_Filter;
        private IEventHubLoggerSettings m_Settings;
     
        public EventHubLoggerProvider(IEventHubLoggerSettings settings, Func<string, LogLevel, bool> filter)
        {
            this.m_Filter = filter;
            this.m_Settings = settings;
        }
        
        public ILogger CreateLogger(string categoryName)
        {
            return m_Loggers.GetOrAdd(categoryName, createLoggerImplementation);
        }

        private EventHubLogger createLoggerImplementation(string categoryName)
        {           
            return new EventHubLogger(m_Settings, categoryName, getFilter(categoryName, m_Settings));
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

            return (n, l) => 
            false;
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
