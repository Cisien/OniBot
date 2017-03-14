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
        private static readonly object stateLock = new object();

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

        private Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg.Channel is SocketGuildChannel channel))
            {
                return Task.CompletedTask;
            }

            if (arg.Content.StartsWith(_globalConfig.PrefixChar.ToString()))
            {
                return Task.CompletedTask;
            }

            if (arg.Author.IsBot)
            {
                return Task.CompletedTask;
            }

            lock (stateLock)
            {
                var guildId = channel.Guild.Id;
                _config.Reload(channel.Guild.Id);

                var channelId = arg.Channel.Id;

                if (!_messagesSinceLastSend.ContainsKey(channelId))
                {
                    _messagesSinceLastSend.Add(channelId, 0);
                }

                if (!_messageToSendOn.ContainsKey(channelId))
                {
                    _messageToSendOn.Add(channelId, _random.Next(_config.MinMessages, _config.MaxMessages));
                }
                _logger.LogDebug($"Randomly: channel: {channelId} ({channel.Name})  current: { _messagesSinceLastSend[channelId]} target: {_messageToSendOn[channelId]}");

                _messagesSinceLastSend[channelId]++;

                if (_messagesSinceLastSend[channelId] < _messageToSendOn[channelId])
                {
                    return Task.CompletedTask;
                }

                var message = _config.RandomMessages.Random();

                if (message.Image == null)
                {
                    _logger.LogDebug("Sending message without attachment");
                    arg.Channel.SendMessageAsync(message.Message).AsSync(false);
                }
                else
                {
                    _logger.LogDebug("Sending message with attachment");
                    var image = client.GetByteArrayAsync(message.Image).AsSync(false);
                    var extension = Path.GetExtension(message.Image);

                    arg.Channel.SendFileAsync(image, extension, message.Message).AsSync(false);
                }

                _messageToSendOn[channelId] = _random.Next(_config.MinMessages, _config.MaxMessages);
                _logger.LogDebug($"Selected a new random message to send on: {_messageToSendOn[channelId]}");
                _messagesSinceLastSend[channelId] = 0;
            }
            return Task.CompletedTask;
        }
    }
}
