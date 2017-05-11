using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventHub;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp.EventHubTest
{
    class Program
    {
        private static string m_Connstr = "Endpoint=sb://eventhubnamespacename3.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey=cEEtxGzkSu4CYKTLXEZ+twKRBM5inArIJlVVkSAXztM=;EntityPath=p1-eventhub";

        static void Main(string[] args)
        {
            Test2(); 

        }

        private static void Test1()
        {
            IEventHubLoggerSettings sett = null;

            var logger = new EventHubLogger(sett, nameof(Program));

            logger.LogInformation(1, "INF Message");
        }

        private static void Test2()
        {

            EventHubLoggerSettings settings = new EventHubLoggerSettings()
            {
                ConnectionString = m_Connstr,
                IncludeScopes = true,
                Switches = new Dictionary<string, LogLevel>()
                { { typeof(Program).FullName, LogLevel.Debug }, }
            };

            string a= nameof(Program);
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddEventHub(settings);

            ILogger logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation(
            "This is a test of the emergency broadcast system.{0}-{1}", 1, "prm2");

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

        private static void Test3()
        {
            EventHubLoggerSettings settings = new EventHubLoggerSettings()
            {
                ConnectionString = "",
                IncludeScopes = true,

            };

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddEventHub(settings, (string a, LogLevel l) =>
                {
                    return true;
                });


            ILogger logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation(
            "This is a test of the emergency broadcast system.");

            logger.LogCritical(new EventId(123, "txt123"), "123 message");
            logger.LogError(new EventId(456, "txt456"), "456 msg");

            var t1 = Task.Run(() =>
            {
                using (logger.BeginScope<string>("MYSCOPE1.0"))
                {
                    logger.LogInformation(DateTime.Now.ToString());

                    using (logger.BeginScope<string>("MYSCOPE1.1"))
                    {
                        logger.LogInformation(DateTime.Now.ToString());
                    }
                }
            });

            var t2 = Task.Run(() =>
            {
                using (logger.BeginScope<string>("MYSCOPE2.0"))
                {
                    logger.LogInformation(DateTime.Now.ToString());
                }
            });

            Console.WriteLine("Hello World!");
        }

    }
}