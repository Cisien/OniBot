using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OniBot
{
    class DiscordBot : IDiscordBot
    {
        private DiscordSocketClient client;
        private  ICommandHandler _commandHandler;
        private static Random random = new Random();
        private Dictionary<string, IBotBehavior> _behaviors;
        private IDependencyMap _depMap;
        private BehaviorService _behaviorService;
        private static ILogger _logger;

        private BotConfig _configuration;

        public DiscordBot(BotConfig config, ICommandHandler commandHandler, IDependencyMap depMap, BehaviorService behaviorService, ILogger logger)
        {
            _configuration = config;
            _depMap = depMap;
            _commandHandler = commandHandler;
            _behaviorService = behaviorService;
            _logger = logger;
            _behaviors = new Dictionary<string, IBotBehavior>();
        }

        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = _configuration.LogLevel,
                AlwaysDownloadUsers = _configuration.AlwaysDownloadUsers,
                MessageCacheSize = _configuration.MessageCacheSize,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            _depMap.Add<IDiscordClient>(client);
            _depMap.Add(client);
            
            client.Ready += OnReadyAsync;
            client.Log += OnLogAsync;
            client.LoggedOut += OnLoggedOutAsync;

            await _commandHandler.InstallAsync(_depMap).ConfigureAwait(false);
            await _behaviorService.InstallAsync().ConfigureAwait(false);

            try
            {
                await DoConnectAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                throw;
            }
        }

        private async Task OnLoggedOutAsync()
        {
            await _behaviorService.StopAsync().ConfigureAwait(false);
        }
        
        private async Task OnReadyAsync()
        {
            await _behaviorService.RunAsync().ConfigureAwait(false);
        }

        private async Task DoConnectAsync()
        {
            var maxAttempts = 10;
            var currentAttempt = 0;
            do
            {
                currentAttempt++;
                try
                {
                    await client.LoginAsync(TokenType.Bot, _configuration.Token).ConfigureAwait(false);
                    await client.StartAsync().ConfigureAwait(false);
                    break;
                }
                catch (Exception ex)
                {
                
                    _logger.LogError($"Fialed to connect: {ex.Message}");
                    await Task.Delay(currentAttempt * 1000).ConfigureAwait(false);
                }
            }
            while (currentAttempt < maxAttempts);
        }

        public static Task OnLogAsync(LogMessage msg)
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

        public void Dispose()
        {
            client?.SetStatusAsync(UserStatus.Offline)?.AsSync(false);
            client?.LogoutAsync()?.AsSync(false);
            client?.StopAsync()?.AsSync(false);
        }
    }
}
