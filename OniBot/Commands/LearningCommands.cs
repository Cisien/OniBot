using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [ConfigurationPrecondition]
    public class LearningCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private ICommandHandler _commandHandler;
        private CustomCommandsConfig _config;

        public LearningCommands(ICommandHandler commandHandler, CustomCommandsConfig config)
        {
            _commandHandler = commandHandler;
            _config = config;
        }

        [Command("learn")]
        [Summary("Teaches the bot a command.")]
        public async Task Learn(
            [Summary("The name of the command to add")]string command,
            [Summary("The value to send as the response whenever this command is used."), Remainder]string response)
        {
            await Configuration.Modify<CustomCommandsConfig>(_config.ConfigKey, async a =>
            {
                if (a.Commands.ContainsKey(command))
                {
                    await ReplyAsync($"Command '{command}' is already known");
                    return;
                }

                a.Commands.Add(command, response);
            }, Context.Guild.Id);

            await _commandHandler.ReloadCommands();
            await ReplyAsync($"I'll remember that {command} is {response}");
        }

        [Command("forget")]
        [Summary("Instructs the bot to forget a command.")]
        public async Task Forget(
            [Summary("The name of the command to forget.")]string command)
        {
            command = command.ToLower();
            await Configuration.Modify<CustomCommandsConfig>(_config.ConfigKey, async a =>
            {
                if (!a.Commands.ContainsKey(command))
                {
                    await ReplyAsync($"I don't know '{command}'.");
                    return;
                }

                a.Commands.Remove(command);
            }, Context.Guild.Id);

            await _commandHandler.ReloadCommands();
            await ReplyAsync($"I seem to have forgotten what {command} does!??");
        }

        [Command("showcustomcommands")]
        [Summary("Shows the current list of custom commands")]
        public async Task Show(
        [Summary("[Optional] The name of the command to show. If ommited, all commands will be shown.")]string command = null)
        {
            command = command?.ToLower();

            string response = string.Empty;
            if (string.IsNullOrWhiteSpace(command))
            {
                response = JsonConvert.SerializeObject(_config, Formatting.Indented);
            }
            else
            {
                if (_config.Commands.ContainsKey(command))
                {
                    response = JsonConvert.SerializeObject(_config.Commands[command], Formatting.Indented);
                }
                else
                {
                    response = $"'{command}' was not found.";
                }
            }
            response = $"```{response}```";
            await Context.User.SendMessageAsync(response);
        }

        [Command("tag")]
        [Summary("Displays any content associated with the command")]
        public async Task CustomCommand(
            [Summary("The command to execute")]string command
        )
        {
            _config.Reload(Context.Guild.Id);
                        
            if (!_config.Commands.ContainsKey(command))
            {
                await ReplyAsync("I'm a little teapot!");
                return;
            }

            await ReplyAsync(_config.Commands[command]);
        }
    }
}
