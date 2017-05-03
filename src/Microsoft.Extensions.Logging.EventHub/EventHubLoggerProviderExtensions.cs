using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Logging.EventHub
{
    public static class EventHubLoggerProviderExtensions
    {
        #region Public Methods

        /// <summary>
        /// Add EventHub with no filter
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="settings"></param>
        /// <param name="filter">Optional filter function, which implements the filter logic.</param>
        /// <remarks>If filter function is specified, all possibly defined switches will be ignored.</remarks>
        /// <returns></returns>
        public static ILoggerFactory AddEventHub(this ILoggerFactory loggerFactory, 
            IEventHubLoggerSettings settings, 
            Func<string, LogLevel, bool> filter = null)
        {
            //if (filter == null)
            //    filter = (n, l) => l >= LogLevel.Information;


            loggerFactory.AddProvider(new EventHubLoggerProvider(settings, filter));

            return loggerFactory;
        }

        public static IEventHubLoggerSettings GetEventHubLoggerSettings(this IConfiguration config)
        {
            EventHubLoggerSettings settings = new EventHubLoggerSettings();

            settings.IncludeScopes = config.GetValue<bool>("IncludeScopes");
            config.GetSection("Switches").Bind(settings.Switches);

            settings.ConnectionString = config.GetSection("EventHub").GetValue<string>("ConnectionString");
            settings.IncludeExceptionStackTrace = config.GetSection("EventHub").GetValue<bool>("IncludeExceptionStackTrace");
            settings.RetryPolicy = getRetryPolicy(config.GetSection("EventHub").GetValue<int>("RetryPolicy"));

            return settings;
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

        /// <summary>
        /// If RetryPolicy in Configuration File is set to 0, then there will be no retry.
        /// All other values will be interpreted as default retry policy.
        /// </summary>
        /// <param name="policyValue">Integer value in IConfiguration</param>
        /// <returns>RetryPolicy type object.</returns>
        private static RetryPolicy getRetryPolicy(int policyValue)
        {
            switch (policyValue)
            {
                case 0:
                    return RetryPolicy.NoRetry;
                default:
                    return RetryPolicy.Default;
            }
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
