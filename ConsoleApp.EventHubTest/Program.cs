using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventHub;
using System;

namespace ConsoleApp.EventHubTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IEventHubLoggerSettings sett = null;
            // Arrange
            var logger = new EventHubLogger(sett);
            var message = "{test string}";

            logger.LogInformation(1, "INF Message");

            // Act
            // logger.Log<string>(LogLevel.Debug, 0, "mystate", null,  message);
        }

        private static void Test2()
        {
            EventHubLoggerSettings settings = new EventHubLoggerSettings()
            {
                ConnectionString = "",
                IncludeScopes = true,
                CategoryName = "CAT1",
            };

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddEventHub(settings, (string a, LogLevel l) =>
            {
                return true;
            });

            //.AddConsole((string a, LogLevel l) =>
            //{
            //    return true;
            //}, true)
            //.AddDebug();
            ILogger logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation(
            "This is a test of the emergency broadcast system.");

            logger.LogCritical(new EventId(123, "txt123"), "123 message");
            logger.LogError(new EventId(456, "txt456"), "456 msg");
            using (logger.BeginScope<string>("MYSCOPE1.0"))
            {
                logger.LogInformation(DateTime.Now.ToString());

                using (logger.BeginScope<string>("MYSCOPE1.1"))
                {
                    logger.LogInformation(DateTime.Now.ToString());
                }
            }

            using (logger.BeginScope<string>("MYSCOPE2.0"))
            {
                logger.LogInformation(DateTime.Now.ToString());
            }

            Console.WriteLine("Hello World!");
        }
    }
}