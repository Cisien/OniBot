using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using System.Threading;
using Discord.WebSocket;
using OniBot.Infrastructure;
using OniBot.CommandConfigs;
using System.Collections.Generic;

namespace OniBot.Behaviors
{
    public class UpdateGameBehavior : IBotBehavior
    {
        public string Name => nameof(UpdateGameBehavior);

        private Timer _timer;
        private static Random _random = new Random();
        private DiscordSocketClient _client;

        private const string _configKey = "updategame";

        public UpdateGameBehavior(IDiscordClient client)
        {
            _client = client as DiscordSocketClient;
        }

        public async Task RunAsync()
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }

            _timer = new Timer(UpdateGame, _client, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(5));
            await Task.Yield();
        }

        private void UpdateGame(object state)
        {
            var client = state as DiscordSocketClient;
            if (client == null)
            {
                DiscordBot.Log(nameof(UpdateGame), LogSeverity.Error, $"client is a {state?.GetType()?.Name ?? "null"}, and is not expected.");
                return;
            }

            var config = Configuration.Get<GamesConfig>(_configKey);
            if (config?.Games == null || config.Games.Count == 0)
            {
                config.Games = new List<string>() { "OxygenNotIncluded" };
            }

            try
            {
                var games = config.Games;
                var index = _random.Next(0, games.Count - 1);
                var game = games[index];
                client.SetGameAsync(game).AsSync(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
                DiscordBot.Log(nameof(UpdateGame), LogSeverity.Critical, ex.ToString());
            }
        }
    }
}
