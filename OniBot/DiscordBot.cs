using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OniBot
{
    class DiscordBot : IDiscordBot
    {
        private DiscordSocketClient client;
        private static ICommandHandler _commandHandler;
        private static Random random = new Random();
        private Dictionary<string, IBotBehavior> _behaviors = new Dictionary<string, IBotBehavior>();
        private IDependencyMap _depMap;
        private BehaviorService _behaviorService;
        private ILogger _logger;

        public static BotConfig Configuration { get; set; }

        public DiscordBot(BotConfig config, ICommandHandler commandHandler, IDependencyMap depMap, BehaviorService behaviorService, ILogger logger)
        {
            Configuration = config;
            _depMap = depMap;
            _commandHandler = commandHandler;
            _behaviorService = behaviorService;
            _logger = logger;
        }

        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Configuration.LogLevel,
                AlwaysDownloadUsers = Configuration.AlwaysDownloadUsers,
                MessageCacheSize = Configuration.MessageCacheSize,
                AudioMode = AudioMode.Outgoing,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            _depMap.Add<IDiscordClient>(client);
            _depMap.Add(client);

            client.Connected += OnConnectedAsync;
            client.Log += OnLogAsync;

            await _commandHandler.InstallAsync(_depMap);
            await _behaviorService.InstallAsync();

            try
            {
                await DoConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                throw;
            }
        }

        private async Task OnConnectedAsync()
        {
            await _behaviorService.RunAsync();
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
                    await client.LoginAsync(TokenType.Bot, Configuration.Token);
                    await client.StartAsync();
                    break;
                }
                catch (Exception ex)
                {
                
                    _logger.LogError($"Fialed to connect: {ex.Message}");
                    await Task.Delay(currentAttempt * 1000);
                }
            }
            while (currentAttempt < maxAttempts);
        }

        private async Task OnLogAsync(LogMessage msg)
        {
            if (msg.Source == "Gateway" && !msg.Message.Contains("Received Dispatch"))
            {
                return;
            }

            var message = $"{msg.Source}: {msg.Message}";

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

            await Task.Yield();
        }

        public void Dispose()
        {
            client?.LogoutAsync()?.AsSync(false);
            client?.StopAsync()?.AsSync(false);
        }
    }
}
