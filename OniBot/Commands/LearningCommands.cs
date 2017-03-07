using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class LearningCommands : ModuleBase, IBotCommand
    {
        private ICommandHandler _commandHandler;
        private string _storageKey = "customcommands";

        public LearningCommands(ICommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [Command("learn")]
        [Summary("Teaches the bot a command.")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task Learn(
            [Summary("The name of the command to add")]string command,
            [Summary("The value to send as the response whenever this command is used.")][Remainder]string response)
        {
            await Configuration.Modify<CustomCommandsConfig>(_storageKey, async a =>
            {
                if (a.Commands.ContainsKey(command))
                {
                    await ReplyAsync($"Command '{command}' is already known");
                    return;
                }

                a.Commands.Add(command, response);
            });

            await _commandHandler.ReloadCommands();
            await ReplyAsync($"I'll remember that {command} is {response}");
        }

        [Command("forget")]
        [Summary("Instructs the bot to forget a command.")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task Forget(
            [Summary("The name of the command to forget.")]string command)
        {
            command = command.ToLower();
            await Configuration.Modify<CustomCommandsConfig>(_storageKey, async a =>
            {
                if (!a.Commands.ContainsKey(command))
                {
                    await ReplyAsync($"I don't know '{command}'.");
                    return;
                }

                a.Commands.Remove(command);
            });

            await _commandHandler.ReloadCommands();
            await ReplyAsync($"I seem to have forgotten what {command} does!??");
        }

        [Command("showcustomcommands")]
        [Summary("Shows the current list of custom commands")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task Show(
        [Summary("[Optional] The name of the command to show. If ommited, all commands will be shown.")]string command = null)
        {
            command = command?.ToLower();
            var customCommands = Configuration.Get<CustomCommandsConfig>(_storageKey);

            string response = string.Empty;
            if (string.IsNullOrWhiteSpace(command))
            {
                response = JsonConvert.SerializeObject(customCommands, Formatting.Indented);
            }
            else
            {
                if (customCommands.Commands.ContainsKey(command))
                {
                    response = JsonConvert.SerializeObject(customCommands.Commands[command], Formatting.Indented);
                }
                else
                {
                    response = $"'{command}' was not found.";
                }
            }
            response = $"```{response}```";
            await Context.User.SendMessageAsync(response);
        }

        [Command("[hidden]customcommands")]
        [CustomCommandAlias("customcommands")]
        [Summary("Displays any content associated with the command")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task CustomCommand()
        {
            var command = Context.Message.Content.Substring(1).ToLower();

            var customCommands = Configuration.Get<CustomCommandsConfig>("customcommands");
            if (!customCommands.Commands.ContainsKey(command))
            {
                await ReplyAsync("I'm a little teapot!");
            }

            await ReplyAsync(customCommands.Commands[command]);
        }
    }
}
