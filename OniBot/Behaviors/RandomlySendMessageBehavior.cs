using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Linq;
using System.IO;

namespace OniBot.Behaviors
{
    public class RandomlySendMessageBehavior : IBotBehavior
    {
        private static Random _random = new Random();
        private int _messagesSinceLastSend = 0;
        private int _messageToSendOn;
        private BotConfig _config;
        private DiscordSocketClient _client;
        private static readonly HttpClient client = new HttpClient();

        public RandomlySendMessageBehavior(IOptions<BotConfig> config)
        {
            _config = config.Value;
        }

        public string Name => nameof(RandomlySendMessageBehavior);

        public async Task RunAsync(IDiscordClient client)
        {
            var discordClient = client as DiscordSocketClient;

            if (client == null)
            {
                DiscordBot.Log(nameof(RunAsync), LogSeverity.Error, $"Discord client is invalid");
                return;
            }
            _client = discordClient;

            _messageToSendOn = _random.Next(_config.MinMessages, _config.MaxMessages);
            discordClient.MessageReceived += OnMessageReceived;
            await Task.Yield();
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            _messagesSinceLastSend++;

            if (_messagesSinceLastSend < _messageToSendOn)
            {
                return;
            }

            var messages = _config.RandomMessages;

            var index = _random.Next(0, messages.Length - 1);
            var message = messages[index];

            if (message.Image == null)
            {

                await arg.Channel.SendMessageAsync(message.Message, false);
            }
            else
            {
                var image = await client.GetByteArrayAsync(message.Image);

                var temp = $"{Guid.NewGuid()}.{message.Image.Split('.').LastOrDefault()}";
                File.WriteAllBytes(temp, image);

                await arg.Channel.SendFileAsync(temp, message.Message);
            }
            _messageToSendOn = _random.Next(_config.MinMessages, _config.MaxMessages);
            _messagesSinceLastSend = 0;
        }
    }
}
