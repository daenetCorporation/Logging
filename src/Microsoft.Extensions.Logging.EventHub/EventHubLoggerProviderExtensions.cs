
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.EventHub
{
    public static class EventHubLoggerProviderExtensions
    {
        #region Public Methods

        /// <summary>
        /// Add EventHub with no filter
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="eventHubName"></param>
        /// <param name="serviceBusNamespace"></param>
        /// <param name="sasToken"></param>
        /// <param name="subSystem"></param>
        /// <returns></returns>
        public static ILoggerFactory AddEventHub(this ILoggerFactory loggerFactory, string eventHubName, string serviceBusNamespace, string sasToken, string subSystem)
        {
            loggerFactory.AddProvider(new EventHubLoggerProvider(eventHubName, serviceBusNamespace, sasToken, subSystem, (n, l) => l >= LogLevel.Information));

            return loggerFactory;
        }

        /// <summary>
        /// Add Eventhub with no filter
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ILoggerFactory AddEventHub(this ILoggerFactory loggerFactory, string connectionString)
        {
            loggerFactory.AddEventHub((n, l) => l >= LogLevel.Information, false, connectionString);

            return loggerFactory;
        }

        /// <summary>
        /// Add EventHub with filter
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="filter"></param>
        /// <param name="includeScopes"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ILoggerFactory AddEventHub(this ILoggerFactory loggerFactory, Func<string, LogLevel, bool> filter, bool includeScopes, string connectionString)
        {
            var tokenData = getTokenData(connectionString);

            var sasToken = createToken(tokenData.HostName, tokenData.KeyName, tokenData.KeySecret);

            loggerFactory.AddProvider(new EventHubLoggerProvider(tokenData.EntityPath, tokenData.HostName, sasToken, null, filter, includeScopes));

            return loggerFactory;
        }

        /// <summary>
        /// Add EventHub with MinLevel filter for all sources
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="minLevel"></param>
        /// <param name="includeScopes"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ILoggerFactory AddEventHub(
            this ILoggerFactory factory,
            LogLevel minLevel,
            bool includeScopes, string connectionString)
        {
            factory.AddEventHub((category, logLevel) => logLevel >= minLevel, includeScopes, connectionString);
            return factory;
        }

        /// <summary>
        /// Add EventHub and load settings from configuration
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ILoggerFactory AddEventHubLogger(this ILoggerFactory loggerFactory, IConfiguration config)
        {
            var settings = new ConfigurationEventHubLoggerSettings(config);

            loggerFactory.AddEventHub(settings);

            return loggerFactory;
        }

        /// <summary>
        /// Add EventHub with settings
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static ILoggerFactory AddEventHub(this ILoggerFactory loggerFactory, IEventHubLoggerSettings settings)
        {
            var tokenData = getTokenData(settings.ConnectionString);

            var sasToken = createToken(tokenData);

            loggerFactory.AddProvider(new EventHubLoggerProvider(tokenData.EntityPath, tokenData.HostName, sasToken, null, settings, settings.IncludeScopes));

            return loggerFactory;
        }

        #endregion


        #region Private Methods

        private static TokenData getTokenData(string connectionString)
        {
            var dict = parseConnectionString(connectionString);

            string endpoint;

            if (dict.ContainsKey("Endpoint"))
                endpoint = dict["Endpoint"];
            else
                throw new ArgumentException($"Couldn't find Endpoint in given EventHub ConnectionString: {connectionString}");

            string hostName = "";
            string serviceBusNameSpace = "";

            if (!String.IsNullOrEmpty(endpoint))
            {
                var uri = new Uri(endpoint);

                hostName = uri.Host;
            }

            var keyName = dict["SharedAccessKeyName"];
            var keySecret = dict["SharedAccessKey"];
            var entityPath = dict["EntityPath"];

            TokenData data = new TokenData
            {
                HostName = hostName,
                ServiceBusNameSpace = serviceBusNameSpace,
                KeyName = keyName,
                KeySecret = keySecret,
                EntityPath = entityPath
            };

            return data;
        }

        private static string createToken(TokenData data)
        {
            return createToken(data.HostName, data.KeyName, data.KeySecret);
        }

        private static string createToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + 3600); //EXPIRES in 1h 
            string stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;

            var signature = Convert.ToBase64String(hashHMACSHA256(UTF8Encoding.UTF8.GetBytes(stringToSign), UTF8Encoding.UTF8.GetBytes(key)));
            var sasToken = String.Format(System.Globalization.CultureInfo.InvariantCulture,
            "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                WebUtility.UrlEncode(resourceUri), WebUtility.UrlEncode(signature), expiry, keyName);

            return sasToken;
        }

        private static byte[] hashHMACSHA256(byte[] data, byte[] key = null)
        {
            System.Security.Cryptography.HMACSHA256 hmac;

            if (key != null)
                hmac = new System.Security.Cryptography.HMACSHA256(key);
            else
                hmac = new System.Security.Cryptography.HMACSHA256();

            var bytes = hmac.ComputeHash(data);

            return bytes;
        }

        private static uint getExpiry(uint tokenLifetimeInSeconds)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            return ((uint)(diff.Ticks / (1000000000 / 100))) + tokenLifetimeInSeconds;
        }

        /// <summary>
        /// splits ConnectionString and returns a dictionary
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        private static Dictionary<string, string> parseConnectionString(string connString)
        {
            Dictionary<string, string> connStringParts = connString.Split(';')
    .Select(t => t.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
    .ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.CurrentCultureIgnoreCase);

            return connStringParts;
        }

        #endregion

        #region Inner Classes

        class TokenData
        {
            public string HostName { get; set; }
            public string KeyName { get; set; }
            public string KeySecret { get; set; }
            public string EntityPath { get; set; }
            public string ServiceBusNameSpace { get; set; }
            public string EventHubName { get; internal set; }
        }

        #endregion
    }
}
