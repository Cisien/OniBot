using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using OniBot.Infrastructure.Help;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace OniBot
{
    internal class CommandHandler : ICommandHandler
    {
        private IServiceProvider _map;
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

        public async Task InstallAsync(IServiceCollection map)
        {
            _map = map.BuildServiceProvider();
            _client = (DiscordSocketClient)_map.GetService(typeof(DiscordSocketClient));

            await LoadAllModules().ConfigureAwait(false);

            _client.MessageReceived += OnMessageReceivedAsync;
            _client.MessageUpdated += OnMessageUpdatedAsync;
        }

        public async Task ReloadCommands()
        {
            foreach (var module in _commands.Modules.ToList())
            {
                await _commands.RemoveModuleAsync(module).ConfigureAwait(false);
            }

            await LoadAllModules().ConfigureAwait(false);
        }

        public async Task<List<Help>> BuildHelpAsync(ICommandContext context)
        {
            var helpList = new List<Help>();

            foreach (var command in _commands.Modules.SelectMany(a => a.Commands))
            {
                var permission = await command.CheckPreconditionsAsync(context, _map).ConfigureAwait(false);
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

        private async Task LoadAllModules()
        {
            var modules = await _commands.AddModulesAsync(Assembly.GetEntryAssembly()).ConfigureAwait(false);
            foreach (var module in modules)
            {
                _logger.LogInformation($"Loaded command: {string.Join(", ", module.Commands.Select(a => a.Name))} from module {module.Name}");
            }
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> existingMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            if (existingMessage.HasValue && existingMessage.Value?.Content == newMessage.Content)
            {
                return;
            }

            await OnMessageReceivedAsync(newMessage).ConfigureAwait(false);
        }

        private async Task OnMessageReceivedAsync(SocketMessage newMessage)
        {
            if(!(newMessage is SocketUserMessage message))
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

            var result = await _commands.ExecuteAsync(context, argPos, _map, MultiMatchHandling.Best).ConfigureAwait(false);

            switch (result)
            {
                case ExecuteResult exResult:
                    if (!exResult.IsSuccess)
                    {
                        _logger.LogError(exResult.Exception);
                    }
                    break;
                case PreconditionResult pResult:
                    _logger.LogInformation(pResult.ErrorReason);
                    await context.User.SendMessageAsync(pResult.ErrorReason).ConfigureAwait(false);
                    break;
            }
#if DEBUG
            if (!result.IsSuccess)
            {
                await message.Channel.SendMessageAsync($"Error: {result.ErrorReason}").ConfigureAwait(false);
            }
#endif
        }
    }
}
