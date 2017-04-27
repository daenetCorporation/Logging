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
        private readonly string m_CategoryName;
        private readonly string m_EventHubName;
        private readonly string m_HostName;
        private readonly string m_SasToken;
        private readonly string m_SubSystem;
        private IEventHubLoggerSettings m_Settings;

        private Func<string, LogLevel, bool> m_Filter;

        private bool m_IncludeScopes;

        private HttpClient m_Client;

        //private EventHubClient m_EventHubClient;

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
            //m_CategoryName = categoryName;
            //m_EventHubName = eventHubName;
            //m_HostName = hostName;
            //m_SasToken = sasToken;
            //m_SubSystem = subSstem;
            //m_IncludeScopes = includeScopes;

            //m_Client = new HttpClient
            //{
            //    BaseAddress = new Uri(string.Format("https://{0}", m_HostName))
            //};
            //m_Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", m_SasToken);

            //m_EventHubClient = EventHubClient.CreateFromConnectionString(m_Settings.ConnectionString);

            //m_EventHubClient.RetryPolicy = new RetryExponential(minBackoff: TimeSpan.FromSeconds(m_Settings.RetryMinBackoffTimeInSec),
            //                                                 maxBackoff: TimeSpan.FromSeconds(m_Settings.RetryMaxBackoffTimeInSec),
            //                                                 maxRetryCount: m_Settings.MaxRetryCount);

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return m_Filter(m_CategoryName, logLevel);
        }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string url;

            if (!String.IsNullOrEmpty(m_SubSystem))
                url = string.Format("{0}/publishers/{1}/messages", m_EventHubName, m_SubSystem);
            else
                url = $"{m_EventHubName}/messages";

            var payload = JsonConvert.SerializeObject(convertToAnonymous(logLevel, eventId, state, exception, formatter));

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            m_Client.PostAsync(url, content);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return EventHubLogScope.Push(m_CategoryName, state);
        }

        #endregion

        #region Private Methods

        private object convertToAnonymous<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string exString = string.Empty;
            if (exception != null)
            {
                exString = formatter(state, exception);
            }

            dynamic data = new System.Dynamic.ExpandoObject();
            data.name = m_CategoryName;
            data.eventId = eventId.ToString();
            data.message = state;
            data.level = logLevel;
            data.time = DateTime.UtcNow.ToString("O");

            if (!String.IsNullOrEmpty(m_SubSystem))
                data.system = m_SubSystem;

            if (!String.IsNullOrEmpty(exString))
                data.exception = exString;

            if (m_IncludeScopes)
                data.scope = getScopeInformation();

            return data;
        }

        private string getScopeInformation()
        {
            StringBuilder builder = new StringBuilder();

            var current = EventHubLogScope.Current;
            string scopeLog = string.Empty;
            var length = builder.Length;

            while (current != null)
            {
                if (length == builder.Length)
                {
                    scopeLog = $"=> {current}";
                }
                else
                {
                    scopeLog = $"=> {current} ";
                }

                builder.Insert(length, scopeLog);
                current = current.Parent;
            }
            //if (builder.Length > length)
            //{
            //    builder.Insert(length);
            //    builder.AppendLine();
            //}
            return builder.ToString();
        }

        #endregion
    }
}
