using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OniBot
{
    internal class CommandHandler
    {
        private IDependencyMap _map;
        private DiscordSocketClient client;
        private CommandService commands;

        public async Task Install(IDependencyMap map)
        {
            _map = map;
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();
            map.Add(commands);

            var modules = await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            foreach (var module in modules)
            {
                DiscordBot.Log($"{nameof(CommandHandler)}.{nameof(Install)}", LogSeverity.Info, $"Loaded command: {string.Join(", ", module.Commands.Select(a => a.Name))} from module {module.Name}");
            }

            client.MessageReceived += OnMessageReceived;
            client.MessageUpdated += OnMessageUpdated;
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> message, SocketMessage socketMessage, ISocketMessageChannel channel)
        {
            if (message.Value.Content == socketMessage.Content)
            {
                return;
            }

            await OnMessageReceived(socketMessage);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null)
            {
                return;
            }

            if (message.Author.IsBot)
            {
                return;
            }

            int argPos = 0;
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasCharPrefix('.', ref argPos)))
            {
                return;
            }

            DiscordBot.Log(nameof(CommandHandler), LogSeverity.Info, $"Command received: {arg.Content}");

            var context = new CommandContext(client, message);

            var result = await commands.ExecuteAsync(context, argPos, _map);

            if (!result.IsSuccess)
            {
                await message.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }
    }
}
