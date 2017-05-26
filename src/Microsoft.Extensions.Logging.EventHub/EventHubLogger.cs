using System;
using System.Text;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging.Internal;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Extensions.Logging.EventHub
{
    public class EventHubLogger : ILogger
    {
        private readonly string m_EventHubName;
        private readonly string m_HostName;
        private readonly string m_SasToken;
        private readonly string m_SubSystem;

        private IEventHubLoggerSettings m_Settings;
        private Func<string, LogLevel, bool> m_Filter;
        private bool m_IncludeScopes;
        private string m_CategoryName;

        private EventHubClient m_EventHubClient;
        private EventHubLogScopeManager m_ScopeManager;

        /// <summary>
        /// List of key value pairs, which will be logged with every message.
        /// </summary>
        private Dictionary<string, object> m_AdditionalValues;

        public Func<LogLevel, EventId, object, Exception, EventData> EventDataFormatter { get; set; }

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="categoryName"></param>
        /// <param name="filter"></param>
        /// <param name="eventDataFormatter"></param>
        /// <param name="additionalValues">List of key value pairs, which will be logged with every message.</param>
        public EventHubLogger(IEventHubLoggerSettings settings, 
            string categoryName, Func<string, LogLevel, bool> filter = null, 
            Func<LogLevel, EventId, object, Exception, EventData> eventDataFormatter = null,
            Dictionary<string, object> additionalValues = null)
        {
            if (filter == null)
                m_Filter = filter ?? ((category, logLevel) => true);
            else
                m_Filter = filter;

            this.m_AdditionalValues = additionalValues;

            m_Settings = settings;

            m_CategoryName = categoryName;

            EventDataFormatter = eventDataFormatter == null ? defaultEventFormatter : eventDataFormatter;

            m_EventHubClient = EventHubClient.CreateFromConnectionString(m_Settings.ConnectionString);

            if (m_Settings.RetryPolicy != null)
                m_EventHubClient.RetryPolicy = m_Settings.RetryPolicy;

        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> exceptionFormatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            EventData ehEvent = EventDataFormatter(logLevel, eventId, state, exception);

            Debug.WriteLine($">>{JsonConvert.SerializeObject(ehEvent)}");

            m_EventHubClient.SendAsync(ehEvent).Wait();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (m_ScopeManager == null)
            {
                m_ScopeManager = new EventHubLogScopeManager(state);
            }

            var scope = m_ScopeManager.Push(state);

            return scope;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return m_Filter(m_CategoryName, logLevel);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Implements default formatter for event data, which will be sent to EventHub.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private EventData defaultEventFormatter(LogLevel logLevel, EventId eventId, object state, Exception exception)
        {
            System.Dynamic.ExpandoObject expando = new System.Dynamic.ExpandoObject();
            var data = (System.Collections.Generic.IDictionary<String, Object>)expando;

            data.Add("Name", m_CategoryName);
            data.Add("Scope", m_ScopeManager == null ? null : m_ScopeManager.Current);
            data.Add("EventId", eventId.ToString());
            data.Add("Message", state.ToString());
            data.Add("Level", logLevel);
            data.Add("LocalEnqueuedTime", DateTime.Now.ToString("O"));
            data.Add("Exception", exception == null ? null : new
            {
                Message = exception.Message,
                Type = exception.GetType().Name,
                StackTrace = exception.StackTrace
            });

            if (this.m_AdditionalValues != null)
            {
                foreach (var item in this.m_AdditionalValues)
                {
                    data.Add(item.Key, item.Value);
                }
            }

            if (state is FormattedLogValues)
            {
                FormattedLogValues v = state as FormattedLogValues;
                foreach (var item in v)
                {
                    data.Add(item.Key, item.Value);
                }
            }

            var payload = JsonConvert.SerializeObject(data);

            EventData eventData = new EventData(Encoding.UTF8.GetBytes(payload));

            System.Diagnostics.Debug.WriteLine(payload);

            return eventData;
        }

        #endregion
    }
}
