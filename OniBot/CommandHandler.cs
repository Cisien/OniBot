using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OniBot
{
    internal class CommandHandler : ICommandHandler
    {
        private IDependencyMap _map;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private BotConfig _config;
        
        public CommandHandler(CommandService commandService, IOptions<BotConfig> config)
        {
            _commands = commandService;
            _config = config.Value;
        }

        public async Task InstallAsync(IDependencyMap map)
        {
            _map = map;
            _client = _map.Get<DiscordSocketClient>();
            map.Add(_commands);
            
            var modules = await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            foreach (var module in modules)
            {
                DiscordBot.Log($"{nameof(CommandHandler)}.{nameof(InstallAsync)}", LogSeverity.Info, $"Loaded command: {string.Join(", ", module.Commands.Select(a => a.Name))} from module {module.Name}");
            }

            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMessageUpdatedAsync;
        }

        public async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> existingMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            if (existingMessage.Value.Content == newMessage.Content)
            {
                return;
            }

            await OnMessageReceivedAsync(newMessage);
        }

        public async Task OnMessageReceivedAsync(SocketMessage newMessage)
        {
            var message = newMessage as SocketUserMessage;
            if (message == null)
            {
                return;
            }

            if (message.Author.IsBot)
            {
                return;
            }

            int argPos = 0;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(_config.PrefixChar, ref argPos)))
            {
                return;
            }

            DiscordBot.Log(nameof(CommandHandler), LogSeverity.Info, $"Command received: {newMessage.Content}");

            var context = new CommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, _map);

            if (!result.IsSuccess)
            {
                await message.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }
    }
}
