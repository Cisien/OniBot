using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading;
using OniBot.Infrastructure;
using OniBot.CommandConfigs;
using System.Net.Http;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;

namespace OniBot.Behaviors
{
    public class AvatarRotatorBehavior : IBotBehavior
    {
        public string Name => nameof(AvatarRotatorBehavior);

        private static Timer _timer;
        private static readonly Random _random = new Random();

        public AvatarRotatorBehavior(IOptions<BotConfig> config) { }

        public async Task RunAsync(IDiscordClient client)
        {
            var discClient = client as DiscordSocketClient;
            _timer = new Timer(UpdateAvatar, client, TimeSpan.FromSeconds(0), TimeSpan.FromHours(24));
            await Task.Yield();
        }

        private void UpdateAvatar(object state)
        {
            var client = state as DiscordSocketClient;

            if (client == null)
            {
                return;
            }

            var config = Configuration.Get<AvatarConfig>("avatar");

            if (config.Avatars == null || config.Avatars.Count == 0)
            {
                return;
            }

            var index = _random.Next(0, config.Avatars.Count);
            var avatar = config.Avatars.ElementAtOrDefault(index);

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
                }
            }
        }
    }
}
