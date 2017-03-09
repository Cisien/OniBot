using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text;
using OniBot.Infrastructure;
using System.Collections.Generic;
using OniBot.Infrastructure.Help;
using Module = OniBot.Infrastructure.Help.Module;

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
            map.Add<ICommandHandler>(this);
            map.Add(_commands);
            map.Add(_config);

            await LoadAllModules();

            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMessageUpdatedAsync;
        }

        public async Task ReloadCommands()
        {
            foreach (var module in _commands.Modules.ToList())
            {
                await _commands.RemoveModuleAsync(module);
            }

            await LoadAllModules();
        }

        public async Task<List<Help>> BuildHelp(ICommandContext context)
        {
            var helpList = new List<Help>();

            var sb = new StringBuilder();
            sb.AppendLine($"{"Command".PadRight(20)}{"Parameters".PadRight(20)}Summary");

            foreach (var command in _commands.Modules.SelectMany(a => a.Commands))
            {
                var help = new Help();

                var permission = await command.CheckPreconditionsAsync(context, _map);
                if (!permission.IsSuccess)
                {
                    continue;
                }

                helpList.Add(help);


                if (command.Aliases.Count == 1)
                {
                    if (command.Name.StartsWith("[hidden]"))
                    {
                        continue;
                    }

                    var cmd = new Command();
                    help.Commands.Add(cmd);
                    cmd.Alias = command.Aliases.FirstOrDefault();
                    cmd.Summary = string.IsNullOrWhiteSpace(command.Summary) ? command.Module.Summary : command.Summary;
                    foreach (var parameter in command.Parameters)
                    {
                        var param = new Parameter();
                        cmd.Parameters.Add(param);
                        param.Name = parameter.Name;
                        param.Summary = parameter.Summary;
                    }
                }
                else
                {
                    foreach (var alias in command.Aliases)
                    {
                        if (alias.StartsWith("[hidden]"))
                        {
                            continue;
                        }
                        var cmd = new Command();
                        help.Commands.Add(cmd);
                        cmd.Alias = alias;
                        cmd.Summary = string.IsNullOrWhiteSpace(command.Summary) ? command.Module.Summary : command.Summary;
                        foreach (var parameter in command.Parameters)
                        {
                            var param = new Parameter();
                            cmd.Parameters.Add(param);
                            param.Name = parameter.Name;
                            param.Summary = parameter.Summary;
                        }
                    }
                }
            }

            await Task.Yield();
            return helpList;
        }

        private bool HasPermission(CommandInfo a, ICommandContext context)
        {
            return a.CheckPreconditionsAsync(context, _map).AsSync(false).IsSuccess;
        }

        private async Task LoadAllModules()
        {
            var modules = await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            foreach (var module in modules)
            {
                DiscordBot.Log($"{nameof(CommandHandler)}.{nameof(InstallAsync)}", LogSeverity.Info, $"Loaded command: {string.Join(", ", module.Commands.Select(a => a.Name))} from module {module.Name}");
            }
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> existingMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            if (existingMessage.Value.Content == newMessage.Content)
            {
                return;
            }

            await OnMessageReceivedAsync(newMessage);
        }

        private async Task OnMessageReceivedAsync(SocketMessage newMessage)
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

            var result = await _commands.ExecuteAsync(context, argPos, _map, MultiMatchHandling.Best);

            if (result is ExecuteResult)
            {
                DiscordBot.Log($"{nameof(CommandHandler)}.{nameof(OnMessageReceivedAsync)}", LogSeverity.Error, ((ExecuteResult)result).Exception.ToString());
            }

#if DEBUG
            if (!result.IsSuccess)
            {
                await message.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
#endif
        }

    }
}
