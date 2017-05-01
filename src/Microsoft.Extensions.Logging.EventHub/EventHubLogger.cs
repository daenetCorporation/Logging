using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        private EventHubClient m_EventHubClient;

        #region Public Methods

        public EventHubLogger(IEventHubLoggerSettings settings, Func<string, LogLevel, bool> filter = null)
        {
            //if (categoryName == null)
            //{
            //    throw new ArgumentNullException(nameof(categoryName));
            //}

            if (filter == null)
                m_Filter = filter ?? ((category, logLevel) => true);
            else
                m_Filter = filter;

            this.m_Settings = settings;

            if (this.m_Settings.EventDataFormatter == null)
            {
                this.m_Settings.EventDataFormatter = defaultEventFormatter;
            }

            m_EventHubClient = EventHubClient.CreateFromConnectionString(m_Settings.ConnectionString);

            if (m_Settings.RetryPolicy != null)
                m_EventHubClient.RetryPolicy = m_Settings.RetryPolicy;

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return m_Filter(this.m_Settings.CategoryName, logLevel);
        }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> exceptionFormatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            EventData ehEvent = m_Settings.EventDataFormatter(logLevel, eventId, state.ToString(), exception);

            //string url;

            //if (!String.IsNullOrEmpty(m_SubSystem))
            //    url = string.Format("{0}/publishers/{1}/messages", m_EventHubName, m_SubSystem);
            //else
            //    url = $"{m_EventHubName}/messages";

            //var payload = JsonConvert.SerializeObject(convertToAnonymous(logLevel, eventId, state, exception, exceptionFormatter));

            //var content = new StringContent(payload, Encoding.UTF8, "application/json");

            //EventData data = new EventData(Encoding.UTF8.GetBytes(payload));

            m_EventHubClient.SendAsync(ehEvent).Wait();
        }


        private EventHubLogScopeManager m_ScopeManager;

        public IDisposable BeginScope<TState>(TState state)
        {
            IDisposable scope;

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (m_ScopeManager == null)
            {
                m_ScopeManager = new EventHubLogScopeManager(state);
            }

            scope = m_ScopeManager.Push(state);

            return scope;
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
        private EventData defaultEventFormatter(LogLevel logLevel, EventId eventId, string message, Exception exception)
        {
            var obj = new
            {
                Name = m_Settings.CategoryName,
                Scope = m_ScopeManager == null ? null : m_ScopeManager.Current,
                EventId = eventId.ToString(),
                Message = message,
                Level = logLevel,
                LocalEnqueuedTime = DateTime.Now.ToString("O"),
                Exception = exception == null ? null : new
                {
                    Message = exception.Message,
                    Type = exception.GetType().Name,
                    StackTrace = exception.StackTrace
                }
            };

            var payload = JsonConvert.SerializeObject(obj);

            EventData data = new EventData(Encoding.UTF8.GetBytes(payload));

            return data;
        }

        private object convertToAnonymous<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> exceptionFormatter)
        {
            string errMsg;

            if (exception != null && exceptionFormatter != null)
                errMsg = exceptionFormatter(state, exception);

            dynamic data = new System.Dynamic.ExpandoObject();
            data.name = m_Settings.CategoryName;
            data.eventId = eventId.ToString();
            data.message = state;
            data.level = logLevel;
            data.time = DateTime.UtcNow.ToString("O");

            if (!String.IsNullOrEmpty(m_SubSystem))
                data.system = m_SubSystem;



            if (m_IncludeScopes)
                data.scope = this.m_ScopeManager.Current;

            return data;
        }


        #endregion
    }
}
