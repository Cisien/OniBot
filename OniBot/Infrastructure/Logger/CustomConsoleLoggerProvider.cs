﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Console.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace OniBot.Infrastructure.Logger
{
    class CustomConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, CustomConsoleLogger> _loggers = new ConcurrentDictionary<string, CustomConsoleLogger>();

        private readonly Func<string, LogLevel, bool> _filter;
        private IConsoleLoggerSettings _settings;
        private readonly CustomConsoleLoggerProcessor _messageQueue = new CustomConsoleLoggerProcessor();

        public CustomConsoleLoggerProvider(Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _settings = new ConsoleLoggerSettings()
            {
                IncludeScopes = includeScopes,
            };
        }

        public CustomConsoleLoggerProvider(IConsoleLoggerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (_settings.ChangeToken != null)
            {
                _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        private void OnConfigurationReload(object state)
        {
            try
            {
                // The settings object needs to change here, because the old one is probably holding on
                // to an old change token.
                _settings = _settings.Reload();

                foreach (var logger in _loggers.Values)
                {
                    logger.Filter = GetFilter(logger.Name, _settings);
                    logger.IncludeScopes = _settings.IncludeScopes;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error while loading configuration changes.{Environment.NewLine}{ex}");
            }
            finally
            {
                // The token will change each time it reloads, so we need to register again.
                if (_settings?.ChangeToken != null)
                {
                    _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
                }
            }
        }

        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private CustomConsoleLogger CreateLoggerImplementation(string name)
        {
            return new CustomConsoleLogger(name, GetFilter(name, _settings), _settings.IncludeScopes, _messageQueue);
        }

        private Func<string, LogLevel, bool> GetFilter(string name, IConsoleLoggerSettings settings)
        {
            if (_filter != null)
            {
                return _filter;
            }

            if (settings != null)
            {
                foreach (var prefix in GetKeyPrefixes(name))
                {
                    if (settings.TryGetSwitch(prefix, out LogLevel level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return (n, l) => false;
        }

        private IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }

        public void Dispose()
        {
            _messageQueue.Dispose();
        }
    }
}
