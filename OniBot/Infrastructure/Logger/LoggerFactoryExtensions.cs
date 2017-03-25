using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;

namespace OniBot.Infrastructure.Logger
{
    public static class CustomLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        public static ILoggerFactory AddCustomConsole(this ILoggerFactory factory)
        {
            return factory.AddCustomConsole(includeScopes: false);
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddCustomConsole(this ILoggerFactory factory, bool includeScopes)
        {
            factory.AddCustomConsole((n, l) => l >= LogLevel.Information, includeScopes);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        public static ILoggerFactory AddCustomConsole(this ILoggerFactory factory, LogLevel minLevel)
        {
            factory.AddCustomConsole(minLevel, includeScopes: false);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddCustomConsole(
            this ILoggerFactory factory,
            LogLevel minLevel,
            bool includeScopes)
        {
            factory.AddCustomConsole((category, logLevel) => logLevel >= minLevel, includeScopes);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="filter"></param>
        public static ILoggerFactory AddCustomConsole(
            this ILoggerFactory factory,
            Func<string, LogLevel, bool> filter)
        {
            factory.AddCustomConsole(filter, includeScopes: false);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="filter"></param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddCustomConsole(
            this ILoggerFactory factory,
            Func<string, LogLevel, bool> filter,
            bool includeScopes)
        {
            factory.AddProvider(new CustomConsoleLoggerProvider(filter, includeScopes));
            return factory;
        }

        public static ILoggerFactory AddCustomConsole(
            this ILoggerFactory factory,
            IConsoleLoggerSettings settings)
        {
            factory.AddProvider(new CustomConsoleLoggerProvider(settings));
            return factory;
        }

        public static ILoggerFactory AddCustomConsole(this ILoggerFactory factory, IConfiguration configuration)
        {
            var settings = new ConfigurationConsoleLoggerSettings(configuration);
            return factory.AddCustomConsole(settings);
        }

    }
}
