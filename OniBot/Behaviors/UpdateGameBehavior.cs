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
        private static readonly Random _random = new Random();
        private readonly DiscordSocketClient _client;

        private readonly ILogger<RandomlySendMessageBehavior> _logger;
        private readonly GamesConfig _config;

        public UpdateGameBehavior(IDiscordClient client, ILogger<RandomlySendMessageBehavior> logger, GamesConfig config)
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


        public Task StopAsync()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;
            return Task.CompletedTask;
        }

        private async void UpdateGame(object state)
        {
            _logger.LogDebug("Update Game beginning");
            if (!(state is DiscordSocketClient client))
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

                if (game == null)
                {
                    return;
                }

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
