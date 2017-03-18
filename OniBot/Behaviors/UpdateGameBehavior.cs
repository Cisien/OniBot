using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using System.Threading;
using Discord.WebSocket;
using OniBot.CommandConfigs;
using Microsoft.Extensions.Logging;

namespace OniBot.Behaviors
{
    public class UpdateGameBehavior : IBotBehavior
    {
        public string Name => nameof(UpdateGameBehavior);

        private Timer _timer;
        private static Random _random = new Random();
        private DiscordSocketClient _client;
        
        private ILogger _logger;
        private GamesConfig _config;

        public UpdateGameBehavior(IDiscordClient client, ILogger logger, GamesConfig config)
        {
            _client = client as DiscordSocketClient;
            _logger = logger;
            _config = config;
        }

        public Task RunAsync()
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }

            _timer = new Timer(UpdateGame, _client, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private async void UpdateGame(object state)
        {
            _logger.LogDebug("Update Game beginning");
            var client = state as DiscordSocketClient;
            if (client == null)
            {
                _logger.LogError($"client is {state?.GetType()?.Name ?? "null"}");
                return;
            }

            _config.Reload();
            if (_config.Games.Count == 0)
            {
                _config.Games.Add("OxygenNotIncluded");
            }

            try
            {
                var game = _config.Games.Random();
                await client.SetGameAsync(game).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            _logger.LogDebug("Update Game finished");
        }
    }
}
