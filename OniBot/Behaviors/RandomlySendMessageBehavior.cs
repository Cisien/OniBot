using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Net.Http;
using System.IO;
using OniBot.Infrastructure;
using System.Collections.Generic;
using OniBot.CommandConfigs;
using Microsoft.Extensions.Logging;

namespace OniBot.Behaviors
{
    public class RandomlySendMessageBehavior : IBotBehavior
    {
        private static Random _random = new Random();
        private Dictionary<ulong, int> _messagesSinceLastSend = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> _messageToSendOn = new Dictionary<ulong, int>();
        private RandomlyConfig _config;
        private static readonly HttpClient client = new HttpClient();
        private BotConfig _globalConfig;
        private DiscordSocketClient _client;
        private const string _configKey = "randomly";
        private ILogger _logger;

        public RandomlySendMessageBehavior(BotConfig config, IDiscordClient client, ILogger logger)
        {
            _globalConfig = config;
            _client = client as DiscordSocketClient;
            _logger = logger;
        }

        public string Name => nameof(RandomlySendMessageBehavior);

        public async Task RunAsync()
        {
            if (client == null)
            {
                _logger.LogError($"Discord client is invalid");
                return;
            }
            _config = Configuration.Get<RandomlyConfig>(_configKey);

            _client.MessageReceived += OnMessageReceived;
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
            _logger.LogDebug($"channel: {channelId}");
            if (!_messagesSinceLastSend.ContainsKey(channelId))
            {
                _messagesSinceLastSend.Add(channelId, 0);
            }
            _logger.LogDebug($"current: {_messagesSinceLastSend[channelId]}");

            if (!_messageToSendOn.ContainsKey(channelId))
            {
                _messageToSendOn.Add(channelId, _random.Next(_config.MinMessages, _config.MaxMessages));
            }
            _logger.LogDebug($"target: {_messagesSinceLastSend[channelId]}");

            _messagesSinceLastSend[channelId]++;

            if (_messagesSinceLastSend[channelId] < _messageToSendOn[channelId])
            {
                return;
            }

            var message = _config.RandomMessages.Random();

            if (message.Image == null)
            {
                _logger.LogDebug("Sending message without attachment");
                await arg.Channel.SendMessageAsync(message.Message);
            }
            else
            {
                _logger.LogDebug("Sending message with attachment");
                var image = await client.GetByteArrayAsync(message.Image);
                var extension = Path.GetExtension(message.Image);

                await arg.Channel.SendFileAsync(image, extension, message.Message);
            }

            _messageToSendOn[channelId] = _random.Next(_config.MinMessages, _config.MaxMessages);
            _logger.LogDebug($"Selected a new random message to send on: {_messageToSendOn[channelId]}");
            _messagesSinceLastSend[channelId] = 0;
            _config = Configuration.Get<RandomlyConfig>(_configKey);
        }
    }
}
