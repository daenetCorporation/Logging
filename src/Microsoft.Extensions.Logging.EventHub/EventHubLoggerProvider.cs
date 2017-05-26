using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Azure.EventHubs;

namespace Microsoft.Extensions.Logging.EventHub
{
    public class EventHubLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, EventHubLogger> m_Loggers = new ConcurrentDictionary<string, EventHubLogger>();

        /// <summary>
        /// List of key value pairs, which will be logged with every message.
        /// </summary>
        private Dictionary<string, object> m_AdditionalValues;
        
        private Func<string, LogLevel, bool> m_Filter;

        private IEventHubLoggerSettings m_Settings;

        private Func<LogLevel, EventId, object, Exception, EventData> m_EventDataFormatter;

        public EventHubLoggerProvider(IEventHubLoggerSettings settings, Func<string, LogLevel, bool> filter, Func<LogLevel, EventId, object, Exception, EventData> eventDataFormatter = null, Dictionary < string, object> additionalValues = null)
        {
            this.m_EventDataFormatter = eventDataFormatter;
            this.m_AdditionalValues = additionalValues;
            this.m_Filter = filter;
            this.m_Settings = settings;
        }
        
        public ILogger CreateLogger(string categoryName)
        {
            return m_Loggers.GetOrAdd(categoryName, createLoggerImplementation);
        }

        private EventHubLogger createLoggerImplementation(string categoryName)
        {           
            return new EventHubLogger(m_Settings, categoryName, getFilter(categoryName, m_Settings), m_EventDataFormatter, m_AdditionalValues);
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
            List<string> names = new List<string>();

            var tokens = name.Split('.');

            names.Add(name);
       
            string currName = name;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < tokens.Length-1; i++)
            {
                sb.Append(tokens[i]);
                names.Add(sb.ToString());
                if(i<tokens.Length-1)
                    sb.Append(".");
            }

            names.Add("Default");

            return names;
        }

        public void Dispose()
        {
        }
    }
}
