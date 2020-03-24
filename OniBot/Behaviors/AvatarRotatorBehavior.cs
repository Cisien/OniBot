using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading;
using System.Net.Http;
using System.IO;
using OniBot.CommandConfigs;
using Microsoft.Extensions.Logging;

namespace OniBot.Behaviors
{
    public class AvatarRotatorBehavior : IBotBehavior
    {
        public string Name => nameof(AvatarRotatorBehavior);

        private static Timer _timer;
        private static readonly Random _random = new Random();
        private readonly DiscordSocketClient _client;

        private readonly AvatarConfig _config;
        private readonly ILogger<AvatarRotatorBehavior> _logger;

        public AvatarRotatorBehavior(IDiscordClient client, AvatarConfig config, ILogger<AvatarRotatorBehavior> logger)
        {
            _client = client as DiscordSocketClient;
            _config = config;
            _logger = logger;
        }

        public Task RunAsync()
        {
            _timer = new Timer(UpdateAvatar, _client, TimeSpan.FromSeconds(0), TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;
            return Task.CompletedTask;
        }

        private async void UpdateAvatar(object state)
        {
            _logger.LogDebug("Update Avatar beginning");

            if (!(state is DiscordSocketClient client))
            {
                return;
            }

            _config.Reload();

            if (_config.Avatars.Count == 0)
            {
                _logger.LogWarning("No avatars found.");
                return;
            }

            var avatar = _config.Avatars.Random();

            using var httpClient = new HttpClient();

            try
            {
                var data = await httpClient.GetByteArrayAsync(avatar.Value).ConfigureAwait(false);
                using var ms = new MemoryStream(data)
                {
                    Position = 0
                };
                await client.CurrentUser.ModifyAsync(a =>
                {
                    a.Avatar = new Image(ms);
                }).ConfigureAwait(false);

                _logger.LogInformation($"Avatar image set to {avatar.Key}: {avatar.Value}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(new EventId(0), ex, ex.Message);
            }

            _logger.LogDebug("Update Avatar done");
        }
    }
}
