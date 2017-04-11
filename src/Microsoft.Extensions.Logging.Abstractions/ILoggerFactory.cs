// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Represents a type used to configure the logging system and create instances of <see cref="ILogger"/> from
    /// the registered <see cref="ILoggerProvider"/>s.
    /// </summary>
    public interface ILoggerFactory : IDisposable
    {
        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        ILogger CreateLogger(string categoryName);

        /// <summary>
        /// <para>
        /// This method is being removed from the ILoggerFactory abstraction, to have similar behavior
        /// use the concrete LoggerFactory class with Microsoft.Extensions.Logging.LoggerFactory.AddProvider(string, ILoggerProvider)
        /// </para>
        /// Adds an <see cref="ILoggerProvider"/> to the logging system.
        /// </summary>
        /// <param name="provider">The <see cref="ILoggerProvider"/>.</param>
        [Obsolete("This method is being removed from the ILoggerFactory abstraction, to have similar behavior" +
            " use the concrete LoggerFactory class with Microsoft.Extensions.Logging.LoggerFactory.AddProvider(string, ILoggerProvider)")]
        void AddProvider(ILoggerProvider provider);
    }
}