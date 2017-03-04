using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OniBot
{
    class DiscordBot : IDiscordBot
    {
        private DiscordSocketClient client;
        private ICommandHandler _commandHandler;
        private static Random random = new Random();
        private Dictionary<string, IBotBehavior> _behaviors = new Dictionary<string, IBotBehavior>();
        public static string[] games;
        private BotConfig _config;
        private IOptions<BotConfig> _optionsConfig;

        public DiscordBot(IOptions<BotConfig> config, ICommandHandler commandHandler)
        {
            _optionsConfig = config;
            _config = config.Value;
            games = _config.Games;

            _commandHandler = commandHandler;
        }

        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = _config.LogLevel,
                AlwaysDownloadUsers = _config.AlwaysDownloadUsers,
                MessageCacheSize = _config.MessageCacheSize,
                AudioMode = AudioMode.Outgoing,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            var map = new DependencyMap();
            map.Add(client);

            await _commandHandler.InstallAsync(map);

            client.Log += OnLogAsync;

            try
            {
                await DoConnect();
            }
            catch (Exception ex)
            {
                Log(nameof(Exception), LogSeverity.Critical, $"{ex}");
                throw;
            }
        }

        public async Task RunBehaviorsAsync()
        {
            var assembly = Assembly.GetEntryAssembly();
            var interfaceType = typeof(IBotBehavior);

            var exportedTypes = assembly.ExportedTypes;

            foreach (var type in exportedTypes)
            {
                try
                {
                    var typeInfo = type.GetTypeInfo();

                    if (typeInfo.IsAssignableFrom(interfaceType) && !typeInfo.IsInterface && !typeInfo.IsAbstract)
                    {
                        var instance = (IBotBehavior)Activator.CreateInstance(type, new object[] { _optionsConfig });

                        _behaviors.Add(instance.Name, instance);
                    }
                }
                catch (Exception ex)
                {
                    Log(nameof(RunBehaviorsAsync), LogSeverity.Error, ex.ToString());
                }
            }

            foreach (var behavior in _behaviors)
            {
                try
                {
                    await behavior.Value.RunAsync(client);
                }
                catch (Exception ex)
                {
                    Log(nameof(RunBehaviorsAsync), LogSeverity.Error, ex.ToString());
                }
            }
        }

        private async Task DoConnect()
        {
            var maxAttempts = 10;
            var currentAttempt = 0;
            do
            {
                currentAttempt++;
                try
                {
                    await client.LoginAsync(TokenType.Bot, _config.Token);
                    await client.StartAsync();
                    break;
                }
                catch (Exception ex)
                {
                    Log(nameof(DoConnect), LogSeverity.Warning, $"Fialed to connect: {ex.Message}");
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
            client?.StopAsync().AsSync(false);
            client?.Dispose();
        }
    }
}
