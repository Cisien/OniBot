using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [ConfigurationPrecondition]
    public class LearningCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private ICommandHandler _commandHandler;
        private TagsConfig _config;

        public LearningCommands(ICommandHandler commandHandler, TagsConfig config)
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
            await Configuration.Modify<TagsConfig>(_config.ConfigKey, async a =>
            {
                if (a.Commands.ContainsKey(command))
                {
                    await this.SafeReplyAsync($"Command '{command}' is already known").ConfigureAwait(false);
                    return;
                }

                a.Commands.Add(command, response);
            }, Context.Guild.Id).ConfigureAwait(false);

            await _commandHandler.ReloadCommands().ConfigureAwait(false);
            await this.SafeReplyAsync($"I'll remember that {command} is {response}").ConfigureAwait(false);
        }

        [Command("forget")]
        [Summary("Instructs the bot to forget a command.")]
        public async Task Forget(
            [Summary("The name of the command to forget.")]string command)
        {
            command = command.ToLower();
            await Configuration.Modify<TagsConfig>(_config.ConfigKey, async a =>
            {
                if (!a.Commands.ContainsKey(command))
                {
                    await this.SafeReplyAsync($"I don't know '{command}'.").ConfigureAwait(false);
                    return;
                }

                a.Commands.Remove(command);
            }, Context.Guild.Id).ConfigureAwait(false);

            await _commandHandler.ReloadCommands().ConfigureAwait(false);
            await this.SafeReplyAsync($"I seem to have forgotten what {command} does!??").ConfigureAwait(false);
        }

        [Command("showcustomcommands")]
        [Summary("Shows the current list of custom commands")]
        public async Task Show(
        [Summary("[Optional] The name of the command to show. If ommited, all commands will be shown.")]string command = null)
        {
            command = command?.ToLower();
            _config.Reload(Context.Guild.Id);
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
            await Context.User.SendMessageAsync(response).ConfigureAwait(false);
        }

        [Command("tag")]
        [Summary("Displays any content associated with the command")]
        public async Task CustomCommand(
            [Summary("The command to execute")]string command)
        {
            _config.Reload(Context.Guild.Id);
            command = command.ToLower();
            if (!_config.Commands.ContainsKey(command))
            {
                await this.SafeReplyAsync("I'm a little teapot!").ConfigureAwait(false);
                return;
            }

            await this.SafeReplyAsync(_config.Commands[command]).ConfigureAwait(false);
        }

        [Command("tags")]
        [Summary("Lists all of the tags available")]
        public async Task ListTags()
        {
            _config.Reload(Context.Guild.Id);

            var tags = string.Join(", ", _config.Commands.Keys.Cast<string>().Select(a => a.ToLower()));

            await ReplyAsync(tags);
        }
    }
}
