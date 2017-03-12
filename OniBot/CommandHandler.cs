using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using OniBot.Infrastructure.Help;
using Microsoft.Extensions.Logging;

namespace OniBot
{
    internal class CommandHandler : ICommandHandler
    {
        private IDependencyMap _map;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private BotConfig _config;
        private ILogger _logger;

        public CommandHandler(CommandService commandService, BotConfig config, ILogger logger)
        {
            _commands = commandService;
            _config = config;
            _logger = logger;
        }

        public async Task InstallAsync(IDependencyMap map)
        {
            _map = map;
            _client = _map.Get<DiscordSocketClient>();

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
                var permission = await command.CheckPreconditionsAsync(context, _map);
                if (!permission.IsSuccess)
                {
                    continue;
                }

                var help = new Help();
                helpList.Add(help);

                if (command.Aliases.Count == 1)
                {
                    var cmd = BuildCommand(command, command.Aliases.FirstOrDefault());
                    if (cmd != null)
                    {
                        help.Commands.Add(cmd);
                    }
                }
                else
                {
                    foreach (var alias in command.Aliases)
                    {
                        var cmd = BuildCommand(command, alias);
                        if (cmd != null)
                        {
                            help.Commands.Add(cmd);
                        }
                    }
                }
            }

            await Task.Yield();
            return helpList;
        }

        private static Command BuildCommand(CommandInfo command, string alias)
        {
            if (alias.StartsWith("[hidden]"))
            {
                return null;
            }

            var cmd = new Command()
            {
                Alias = alias,
                Summary = string.IsNullOrWhiteSpace(command.Summary) ? command.Module.Summary : command.Summary
            };

            foreach (var parameter in command.Parameters)
            {
                var param = new Parameter();
                cmd.Parameters.Add(param);
                param.Name = parameter.Name;
                param.Summary = parameter.Summary;
            }

            return cmd;
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
                _logger.LogInformation($"Loaded command: {string.Join(", ", module.Commands.Select(a => a.Name))} from module {module.Name}");
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

            _logger.LogInformation($"Command received: {newMessage.Content}");

            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, _map, MultiMatchHandling.Best);

            switch (result)
            {
                case ExecuteResult exResult:
                    _logger.LogError(exResult.Exception);
                    break;
                case PreconditionResult pResult:
                    _logger.LogInformation(pResult.ErrorReason);
                    await context.User.SendMessageAsync(pResult.ErrorReason);
                    break;
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
