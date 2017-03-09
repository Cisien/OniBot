using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
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
        private IOptions<BotConfig> _optionsConfig;
        private IDependencyMap _depMap;
        private BehaviorService _behaviorService;

        public static BotConfig Configuration { get; set; }

        public DiscordBot(IOptions<BotConfig> config, ICommandHandler commandHandler, IDependencyMap depMap, BehaviorService behaviorService)
        {
            _optionsConfig = config;
            Configuration = config.Value;
            _depMap = depMap;
            _depMap.Add(config);
            _commandHandler = commandHandler;
            _behaviorService = behaviorService;
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
            _depMap.Add(_behaviorService);
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
                Log(nameof(Exception), LogSeverity.Critical, $"{ex}");
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
                    Log(nameof(DoConnectAsync), LogSeverity.Warning, $"Fialed to connect: {ex.Message}");
                    await Task.Delay(currentAttempt * 1000);
                }
            }
            while (currentAttempt < maxAttempts);
        }

        private async Task OnLogAsync(LogMessage msg)
        {
            if (msg.Exception != null)
            {
                Log(msg.Source, msg.Severity, msg.Exception.ToString());
            }
            else
            {
                Log(msg.Source, msg.Severity, msg.Message);
            }

            await Task.Yield();
        }

        public static void Log(string source, LogSeverity sev, string message)
        {
            if (source == "Gateway" && !message.Contains("Received Dispatch"))
            {
                return;
            }

            switch (sev)
            {
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.WriteLine($"{DateTime.Now.ToString("o")} {source}: {sev}: {message}");
        }

        public void Dispose()
        {
            client?.LogoutAsync()?.AsSync(false);
            client?.StopAsync()?.AsSync(false);
        }
    }
}
