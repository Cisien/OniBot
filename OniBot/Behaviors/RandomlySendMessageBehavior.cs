using OniBot.Interfaces;
using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Net.Http;
using System.IO;
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
        private ILogger _logger;

        public RandomlySendMessageBehavior(BotConfig config, IDiscordClient client, ILogger logger, RandomlyConfig randomlyConfig)
        {
            _globalConfig = config;
            _client = client as DiscordSocketClient;
            _logger = logger;
            _config = randomlyConfig;
        }

        public string Name => nameof(RandomlySendMessageBehavior);

        public Task RunAsync()
        {
            if (client == null)
            {
                throw new InvalidOperationException("Client is not valid");
            }

            _client.MessageReceived += OnMessageReceived;
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg.Channel is SocketGuildChannel channel))
            {
                return;
            }

            if (arg.Content.StartsWith(_globalConfig.PrefixChar.ToString()))
            {
                return;
            }

            if (arg.Author.IsBot)
            {
                return;
            }

            var guildId = channel.Guild.Id;
            _config.Reload(channel.Guild.Id);

            if(!_config.Enabled)
            {
                _logger.LogDebug($"{Name} is disabled for guild {guildId}");
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
        }

    }
}
