using OniBot.Interfaces;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using OniBot.Infrastructure;
using System.Linq;
using System.Text;

namespace OniBot.Commands
{
    public class HelpCommand : ModuleBase, IBotCommand
    {
        private ICommandHandler _commandHandler;
        private const int _pageSize = 10;
        private BotConfig _config;

        public HelpCommand(ICommandHandler commandHandler, BotConfig config)
        {
            _commandHandler = commandHandler;
            _config = config;
        }

        [Command("help"), Priority(50)]
        [Summary("Prints the list of commands you have permission to execute.")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task Help(
            [Summary("[Optional] The page to load.")]int page = 1)
        {
            var help = await _commandHandler.BuildHelp(Context);
            var resultMax = (_pageSize - help.Count % _pageSize) + help.Count;
            var pages = resultMax / _pageSize;

            if (page < 1 && page > pages)
            {
                page = 1;
            }

            var helpPage = help.Skip((page - 1) * _pageSize).Take(_pageSize);

            var helpText = new StringBuilder();

            helpText.AppendLine($"{Format.Bold("Command").PadRight(20)}{Format.Bold("Parameters").PadRight(20)}{Format.Bold("Summary")}");
            foreach (var helpItem in helpPage.SelectMany(a => a.Commands))
            {
                var command = $"{_config.PrefixChar}{helpItem.Alias}";
                helpText.AppendLine($"{command.PadRight(20)}{string.Join(", ", helpItem.Parameters.Select(a => a.Name)).PadRight(20)}{helpItem.Summary}");
            }
           
            helpText.AppendLine($"Page {page} of {pages}. Use {_config.PrefixChar}help # to view the other pages.");

            await Context.User.SendMessageAsync($"```{helpText}```");
        }

        [Command("help"), Priority(100)]
        [Summary("Prints the help of a specific command")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task Help([Summary("[Optional] The name of the command to view the help of.")][Remainder]string command)
        {
            if (command.StartsWith(_config.PrefixChar.ToString()))
            {
                command = command.Substring(0);
            }

            var help = await _commandHandler.BuildHelp(Context);
            var cmds = help.SelectMany(a => a.Commands).Where(a => a.Alias.Contains(command));
            
            var sb = new StringBuilder();
            foreach(var cmd in cmds)
            {
                sb.AppendLine($"{cmd.Alias.PadRight(20)}{cmd.Summary}");
                sb.AppendLine($"{Format.Bold("Parameter").PadRight(20)}{Format.Bold("Summary")}");
                if (cmd.Parameters.Count > 0)
                {
                    foreach (var param in cmd.Parameters)
                    {
                        sb.AppendLine($"{param.Name.PadRight(20)}{param.Summary}");
                    }
                }
                sb.AppendLine();
            }

            await Context.User.SendMessageAsync($"```{sb}```");
        }
    }
}
