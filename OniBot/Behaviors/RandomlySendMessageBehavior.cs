using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace OniBot.Behaviors
{
    class RandomlySendMessageBehavior : IBotBehavior
    {
        private static Random _random = new Random();
        private int _messagesSinceLastSend = 0;
        private int _messageToSendOn;
        private BotConfig _config;
        private DiscordSocketClient _client;
        
        public RandomlySendMessageBehavior(IOptions<BotConfig> config) {
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

            _messageToSendOn = _random.Next(50, 101);
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
            await arg.Channel.SendMessageAsync(message);

        }
    }
}
