using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class LearningCommands : ModuleBase, IBotCommand
    {
        ICommandHandler _commandHandler;

        public LearningCommands(ICommandHandler commandHandler) {
            _commandHandler = commandHandler;
        }

        [Command("learn")]
        [Summary("Teaches the bot a command.")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task Learn(string command, [Remainder]string response)
        {
            var customCommands = Configuration.Get<CustomCommandsConfig>("customcommands");
            command = command.ToLower();
            if (customCommands.Commands.ContainsKey(command))
            {
                await ReplyAsync($"Command '{command}' is already known");
                return;
            }

            customCommands.Commands.Add(command, response);
            Configuration.Write(customCommands, "customcommands");
            await _commandHandler.ReloadCommands();
            await ReplyAsync($"I'll remember that {command} is {response}");
        }

        [Command("forget")]
        [Summary("Instructs the bot to forget a command.")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task Forget(string command)
        {
            var customCommands = Configuration.Get<CustomCommandsConfig>("customcommands");
            command = command.ToLower();
            if (!customCommands.Commands.ContainsKey(command))
            {
                await ReplyAsync($"I don't know '{command}'.");
                return;
            }

            customCommands.Commands.Remove(customCommands.Commands[command]);
            Configuration.Write(customCommands, "customcommands");
            await _commandHandler.ReloadCommands();
            await ReplyAsync($"I seem to have forgotten what {command} does!??");
        }

        [Command("showcustomcommands")]
        [Summary("Shows the current list of custom commands")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task Show(string command = null)
        {
            var customCommands = Configuration.Get<CustomCommandsConfig>("customcommands");
            var dmChannel = await Context.User.CreateDMChannelAsync();

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
            await dmChannel.SendMessageAsync(response);
        }

        [Command("customcommands")]
        [DynamcCommandAlias("customcommands")]
        [Summary("Displays any content associated with the command")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task CustomCommand()
        {
            var command = Context.Message.Content.Substring(1).ToLower();
        
            var customCommands = Configuration.Get<CustomCommandsConfig>("customcommands");
            if(!customCommands.Commands.ContainsKey(command)) {
                await ReplyAsync("I'm a little teapot!");
            }

            await ReplyAsync(customCommands.Commands[command]);
        }
    }
}
