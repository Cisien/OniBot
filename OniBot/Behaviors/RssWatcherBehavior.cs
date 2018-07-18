using OniBot.Interfaces;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Net.Http;

namespace OniBot.Behaviors
{
    class RssWatcherBehavior : IBotBehavior
    {
        private static HttpClient _client = new HttpClient();

        public string Name => nameof(RssWatcherBehavior);

        public Task RunAsync()
        {
            //_timer = new Timer(UpdateFeeds, client, TimeSpan.FromMinutes(config.CheckFrequencyMinutes), TimeSpan.FromMinutes(config.CheckFrequencyMinutes));
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
        private static void UpdateFeeds(object state)
        {
            var client = state as DiscordSocketClient;


            //_timer.Change(TimeSpan.FromMinutes(config.CheckFrequencyMinutes), TimeSpan.FromMinutes(config.CheckFrequencyMinutes));
        }
    }
}
