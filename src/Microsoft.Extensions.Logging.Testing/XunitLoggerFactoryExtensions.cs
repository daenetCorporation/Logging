// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging.Testing;
using Xunit.Abstractions;

namespace Microsoft.Extensions.Logging
{
    public static class XunitLoggerFactoryExtensions
    {
        public static LoggerFactory AddXunit(this LoggerFactory loggerFactory, ITestOutputHelper output)
        {
            loggerFactory.AddProvider("Xunit", new XunitLoggerProvider(output));
            return loggerFactory;
        }

        public static LoggerFactory AddXunit(this LoggerFactory loggerFactory, ITestOutputHelper output, LogLevel minLevel)
        {
            loggerFactory.AddProvider("Xunit", new XunitLoggerProvider(output, minLevel));
            return loggerFactory;
        }

        [Obsolete("This method is obsolete, use AddXunit on the concrete Microsoft.Extensions.Logging.LoggerFactory class.")]
        public static ILoggerFactory AddXunit(this ILoggerFactory loggerFactory, ITestOutputHelper output)
        {
            loggerFactory.AddProvider(new XunitLoggerProvider(output));
            return loggerFactory;
        }

        [Obsolete("This method is obsolete, use AddXunit on the concrete Microsoft.Extensions.Logging.LoggerFactory class.")]
        public static ILoggerFactory AddXunit(this ILoggerFactory loggerFactory, ITestOutputHelper output, LogLevel minLevel)
        {
            loggerFactory.AddProvider(new XunitLoggerProvider(output, minLevel));
            return loggerFactory;
        }
    }
}
