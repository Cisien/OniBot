using Cleverbot.Net;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OniBot.CommandConfigs;
using OniBot.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace OniBot.Behaviors
{
    public class ChatbotBehavior : IBotBehavior
    {
        private DiscordSocketClient _client;
        private CleverbotSession _cleverBot;
        private ILogger _logger;
        private BotConfig _globalConfig;
        private ChatConfig _config;
        private static object conversationId = null;
        

        public ChatbotBehavior(IDiscordClient client, ILogger logger, BotConfig globalConfig, ChatConfig config)
        {
            _client = client as DiscordSocketClient;
            _logger = logger;
            _globalConfig = globalConfig;
            _config = config;
        }
        public string Name => nameof(ChatbotBehavior);

        public Task RunAsync()
        {
            _cleverBot = new CleverbotSession(_globalConfig.CleverbotKey, false);
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageReceived += OnMessageReceivedAsync;

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(SocketMessage msg)
        {
            if (msg.Author.IsBot)
            {
                return;
            }

            if (!msg.MentionedUsers.Any(a => a.Id == _client.CurrentUser.Id))
            {
                return;
            }

            if (!(msg.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            _config.Reload(guildChannel.Guild.Id);

            if (!_config.AllowedChannels.Contains(msg.Channel.Id))
            {
                return;
            }
            
            var message = msg.Content.Replace("!", string.Empty).Replace(_client.CurrentUser.Mention, string.Empty).Trim();
            var response = await _cleverBot.GetResponseAsync(message, conversationId?.ToString() ?? string.Empty);

            if (string.IsNullOrWhiteSpace(response.errorLine))
            {
                await msg.Channel.SendMessageAsync(response.Response);
            }
            else
            {
                _logger.LogInformation(response.errorLine);
                await msg.Channel.SendMessageAsync("I forgot what we were talking about.");
            }
            Interlocked.Exchange(ref conversationId, response.ConversationId);
        }
    }
}
