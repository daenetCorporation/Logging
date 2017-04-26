using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
//using Microsoft.Extensions.Logging.EventHub;

namespace Microsoft.Extensions.Logging.Test
{
    public class EventHubLoggerTest
    {
        [Fact]
        public void CallingLogWithCurlyBracesAfterFormatter_DoesNotThrow()
        {
            //// Arrange
            //var logger = new Microsoft.Extensions.Logging.EventHub.ConfigurationEventHubLoggerSettings EventHubLogger("Test");
            //var message = "{test string}";

            //// Act
            //logger.Log(LogLevel.Debug, 0, message, null, (s, e) => s);
        }
    }
}
