using Discord.Commands;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class EchoCommand : ModuleBase, IBotCommand
    {
        [Command("echo", RunMode = RunMode.Async)]
        [Summary("Repeats the text that follows")]
        public async Task Echo([Remainder]string message)
        {
            await ReplyAsync(message);
        }
    }
}
