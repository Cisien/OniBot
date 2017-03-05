using Discord.Commands;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class EchoCommand : ModuleBase, IBotCommand
    {
        [Command("echo")]
        [Summary("Repeats the text that follows")]
        [RequireOwner]
        public async Task Echo([Remainder]string message)
        {
            await ReplyAsync(message);
        }
    }
}
