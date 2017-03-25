using Discord;
using Discord.WebSocket;
using JamesWright.PersonalityForge;
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
        private PersonalityForge _forge;
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
            _forge = new PersonalityForge(_globalConfig.ForgeSecret, _globalConfig.ForgeKey, _globalConfig.BotId);
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
            var response = await _forge.SendAsync(arg.Author.Username, message);

            if (response.Success == 1)
            {
                if (response.Message.Text.ToLower().Contains("nigger"))
                {
                    await arg.Channel.SendMessageAsync("7-second tape delay.");
                    return;
                }
                await arg.Channel.SendMessageAsync(response.Message.Text);
            }
            else
            {
                _logger.LogInformation(response.ToJson());
                await arg.Channel.SendMessageAsync("I forgot what we were talking about.");
            }
        }
    }
}
