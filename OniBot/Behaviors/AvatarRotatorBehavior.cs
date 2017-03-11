using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading;
using OniBot.Infrastructure;
using System.Net.Http;
using System.IO;
using System.Linq;
using OniBot.CommandConfigs;
using Microsoft.Extensions.Logging;

namespace OniBot.Behaviors
{
    public class AvatarRotatorBehavior : IBotBehavior
    {
        public string Name => nameof(AvatarRotatorBehavior);

        private static Timer _timer;
        private static readonly Random _random = new Random();
        private DiscordSocketClient _client;
        
        private AvatarConfig _config;
        private ILogger _logger;

        public AvatarRotatorBehavior(IDiscordClient client, AvatarConfig config, ILogger logger)
        {
            _client = client as DiscordSocketClient;
            _config = config;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            _timer = new Timer(UpdateAvatar, _client, TimeSpan.FromSeconds(0), TimeSpan.FromHours(24));
            await Task.Yield();
        }

        private void UpdateAvatar(object state)
        {
            _logger.LogDebug("Update Avatar beginning");
            var client = state as DiscordSocketClient;

            if (client == null)
            {
                return;
            }
            _config.Reload();
            
            if (_config.Avatars == null || _config.Avatars.Count == 0)
            {
                _logger.LogWarning("No avatars found.");
                return;
            }

            var index = _random.Next(0, _config.Avatars.Count);
            var avatar = _config.Avatars.ElementAtOrDefault(index);

            using (var httpClient = new HttpClient())
            {
                var data = httpClient.GetByteArrayAsync(avatar.Value).AsSync(false);
                using (var ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    client.CurrentUser.ModifyAsync(a =>
                    {
                        a.Avatar = new Image(ms);
                    }).AsSync(false);

                    _logger.LogInformation($"Avatar image set to {avatar.Key}:{avatar.Value}");
                }
            }
            _logger.LogDebug("Update Avatar done");
        }
    }
}
