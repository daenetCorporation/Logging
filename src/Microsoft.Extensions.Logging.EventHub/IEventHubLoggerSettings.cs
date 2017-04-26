using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.EventHub
{
    public interface IEventHubLoggerSettings
    {
        bool IncludeScopes { get; }

        string ConnectionString { get; }

        bool TryGetSwitch(string name, out LogLevel level);
    }
}
