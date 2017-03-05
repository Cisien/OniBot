using OniBot.Interfaces;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class HelpCommand : ModuleBase, IBotCommand
    {
        private ICommandHandler _commandHandler;

        public HelpCommand(ICommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [Command("help")]
        [Summary("Prints the command's help message")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task Help(
            [Summary("[Optional] The name of the command to view the help of.")]string command = null)
        {
            var helpText = await _commandHandler.PrintCommands(Context, command);
            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"```{helpText}```");
        }
    }
}
