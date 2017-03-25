using Cleverbot.Net;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OniBot.CommandConfigs;
using OniBot.Interfaces;
using System.Linq;
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
            _cleverBot = new CleverbotSession(_globalConfig.CleverbotKey);
            _client.MessageReceived += OnMessageReceivedAsync;

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(SocketMessage arg)
        {
            if (arg.Author.IsBot)
            {
                return;
            }

            if (!arg.MentionedUsers.Any(a => a.Mention == _client.CurrentUser.Mention))
            {
                return;
            }

            if (!(arg.Channel is SocketGuildChannel guildChannel))
            {
                return;
            }

            _config.Reload(guildChannel.Guild.Id);

            if (!_config.AllowedChannels.Contains(arg.Channel.Id))
            {
                return;
            }

            var message = arg.Content.Replace(_client.CurrentUser.Mention, string.Empty).Trim();
            var response = await _cleverBot.GetResponseAsync(message);

            if (string.IsNullOrWhiteSpace(response.errorLine))
            {
                await arg.Channel.SendMessageAsync(response.Response);
            }
            else
            {
                _logger.LogInformation(response.errorLine);
                await arg.Channel.SendMessageAsync("I forgot what we were talking about.");
            }
        }
    }
}
