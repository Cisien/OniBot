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
        public async Task Help()
        {
            var helpText = await _commandHandler.PrintCommands(Context);
            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"```{helpText}```");
        }
    }
}
