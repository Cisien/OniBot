using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    public class DiscordLogger<T>
    {
        private ILogger<T> _logger;
        public DiscordLogger(ILogger<T> logger)
        {
            _logger = logger;
        }
        
        public Task OnLogAsync(LogMessage msg)
        {
            var message = msg.ToString();

            switch (msg.Severity)
            {
                case LogSeverity.Info:
                    _logger.LogInformation(message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    if (msg.Exception != null)
                    {
                        _logger.LogError(msg.Exception);
                    }
                    else
                    {
                        _logger.LogError(message);
                    }
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    if (msg.Exception != null)
                    {
                        _logger.LogDebug(msg.Exception);
                    }
                    else
                    {
                        _logger.LogDebug(message);
                    }
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
