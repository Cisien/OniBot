using OniBot.Interfaces;
using Discord;
using System.Threading.Tasks;
using OniBot.Infrastructure;
using OniBot.CommandConfigs;
using System.Threading;
using System;
using Discord.WebSocket;
using System.Net.Http;

namespace OniBot.Behaviors
{
    class RssWatcherBehavior : IBotBehavior
    {
        private static Timer _timer;
        private static HttpClient _client = new HttpClient();

        public string Name => nameof(RssWatcherBehavior);

        public async Task RunAsync(IDiscordClient client)
        {
            var config = Configuration.Get<RssWatcherConfig>("rsswatcher");
            //_timer = new Timer(UpdateFeeds, client, TimeSpan.FromMinutes(config.CheckFrequencyMinutes), TimeSpan.FromMinutes(config.CheckFrequencyMinutes));
            await Task.Yield();
        }


        private static void UpdateFeeds(object state)
        {
            var client = state as DiscordSocketClient;
            var config = Configuration.Get<RssWatcherConfig>("rsswatcher");
            

            //_timer.Change(TimeSpan.FromMinutes(config.CheckFrequencyMinutes), TimeSpan.FromMinutes(config.CheckFrequencyMinutes));
        }
    }
}
