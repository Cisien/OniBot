using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using OniBot.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OniBot
{    class DiscordBot : IDisposable
    {
        private DiscordSocketClient client;
        private CommandHandler commandHandler;
        private static Random random = new Random();
        private Timer timer;
        public static string[] games;
        private IDiscordBotConfig _config;

        public DiscordBot(IOptions<DiscordBotConfig> config)
        {
            _config = config.Value;

            games = _config.Games;
        }

        public async Task Run()
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

            commandHandler = new CommandHandler();
            await commandHandler.Install(map);

            client.Log += OnLogAsync;

            try
            {
                await DoConnect();
                timer = new Timer(UpdateGame, client, TimeSpan.FromMilliseconds(0), TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                Log(nameof(Exception), LogSeverity.Critical, $"{ex}");
                throw;
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

            await Task.CompletedTask;
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

        internal void UpdateGame(object state)
        {
            var client = state as DiscordSocketClient;
            try
            {
                client.SetGameAsync(games[random.Next(0, games.Length - 1)]).AsSync(false);
            }
            catch (Exception ex)
            {
                Log(nameof(UpdateGame), LogSeverity.Critical, ex.ToString());
            }
        }

        public void Dispose()
        {
            client?.StopAsync().AsSync(false);
            client?.Dispose();
        }
    }
}
