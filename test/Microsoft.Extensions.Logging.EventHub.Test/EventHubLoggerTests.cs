using System;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.Azure.EventHubs;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.EventHub.Test
{
    public class EventHubLoggerTests
    {
        private ILogger m_Logger;

        public EventHubLoggerTests()
        {
            initializeEventHubLogger(null);
        }

        [Fact(DisplayName= "EH_LogsAllTypesNoFilter")]
        public void LogsAllTypesNoFilter()
        {
            m_Logger.LogTrace("Test Trace Log Message");
            m_Logger.LogDebug("Test Debug Log Message");
            m_Logger.LogInformation("Test Information Log Message");
            m_Logger.LogWarning("Test Warning Log Message");
            m_Logger.LogError(new EventId(456, "txt456"), "456 Test Error Log Message");
            m_Logger.LogCritical(new EventId(123, "txt123"), "123 Test Critical Log Message");
        }


        /// <summary>
        /// Tests logging of formatted messages.
        /// </summary>
        [Fact(DisplayName = "EH_LogWithFormat")]
        public void LogWithFormat()
        {          
            m_Logger.LogTrace("{PRM1}, Test {PRM2} Log Message", 1, "2");
        }


        /// <summary>
        /// Tests logging of formatted messages.
        /// </summary>
        [Fact(DisplayName = "EH_LogWithAdditionalData")]
        public void LogWithAdditionalData()
        {
            initializeEventHubLogger(null, null, new Dictionary<string, object>()
            {
                { "HOSTNAME", "HostName"},
                { "SomeInt", 42},
            });

            m_Logger.LogTrace("{PRM1}, Test {PRM2} Log Message", 1, "2");
        }


        [Fact(DisplayName = "EH_LogsInformationWithScopesNoFilter")]
        public void LogsInformationWithScopesNoFilter()
        {
            using (m_Logger.BeginScope<string>("MYSCOPE1.0"))
            {
                m_Logger.LogInformation(DateTime.Now.ToString());

                using (m_Logger.BeginScope<string>("MYSCOPE1.1"))
                {
                    m_Logger.LogInformation(DateTime.Now.ToString());
                }
            }

            using (m_Logger.BeginScope<string>("MYSCOPE2.0"))
            {
                m_Logger.LogInformation(DateTime.Now.ToString());
            }
        }

        private void initializeEventHubLogger(Func<string, LogLevel, bool> filter,
              Func<LogLevel, EventId, object, Exception, EventData> eventDataFormatter = null,
           Dictionary<string, object> additionalValues = null)
        {
            ConfigurationBuilder cfgBuilder = new ConfigurationBuilder();
            cfgBuilder.AddJsonFile(@"EventHubLoggerSettings.json");
            var configRoot = cfgBuilder.Build();

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddEventHub(configRoot.GetEventHubLoggerSettings(), filter, eventDataFormatter, additionalValues);
           
            m_Logger = loggerFactory.CreateLogger<EventHubLoggerTests>();
        }
    }
}
