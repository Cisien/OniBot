using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Linq;
using System.IO;
using OniBot.Infrastructure;
using System.Collections.Generic;
using OniBot.CommandConfigs;

namespace OniBot.Behaviors
{
    public class RandomlySendMessageBehavior : IBotBehavior
    {
        private static Random _random = new Random();
        private Dictionary<ulong, int> _messagesSinceLastSend = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> _messageToSendOn = new Dictionary<ulong, int>();
        private RandomlyConfig _config;
        private DiscordSocketClient _client;
        private BotConfig _globalConfig;
        private static readonly HttpClient client = new HttpClient();

        public RandomlySendMessageBehavior(IOptions<BotConfig> config)
        {
            _globalConfig = config.Value;
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
            _config = Configuration.Get<RandomlyConfig>("randomly");
            _client = discordClient;

            discordClient.MessageReceived += OnMessageReceived;
            await Task.Yield();
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg.Content.StartsWith(_globalConfig.PrefixChar.ToString()))
            {
                return;
            }

            if (arg.Author.IsBot)
            {
                return;
            }

            var channelId = arg.Channel.Id;
            if (!_messagesSinceLastSend.ContainsKey(channelId))
            {
                _messagesSinceLastSend.Add(channelId, 0);
            }

            if (!_messageToSendOn.ContainsKey(channelId))
            {
                _messageToSendOn.Add(channelId, _random.Next(_config.MinMessages, _config.MaxMessages));
            }

            _messagesSinceLastSend[channelId]++;

            if (_messagesSinceLastSend[channelId] < _messageToSendOn[channelId])
            {
                return;
            }

            var messages = _config.RandomMessages;

            var index = _random.Next(0, messages.Count - 1);
            var message = messages[index];

            if (message.Image == null)
            {

                await arg.Channel.SendMessageAsync(message.Message);
            }
            else
            {
                var image = await client.GetByteArrayAsync(message.Image);

                var temp = $"{Guid.NewGuid()}{Path.GetExtension(message.Image)}";

                using (var ms = new MemoryStream(image))
                {
                    await arg.Channel.SendFileAsync(ms, temp, message.Message);
                }
            }
            _messageToSendOn[channelId] = _random.Next(_config.MinMessages, _config.MaxMessages);
            _messagesSinceLastSend[channelId] = 0;
            _config = Configuration.Get<RandomlyConfig>("randomly");
        }
    }
}
