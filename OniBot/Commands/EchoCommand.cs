using Discord.Commands;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    class EchoCommand : ModuleBase, IBotCommand
    {
        [Command("echo", RunMode = RunMode.Async)]
        [Summary("Repeats the text that follows")]
        public async Task Echo()
        {
            await ReplyAsync(Context.Message.Content);
        }
    }
}
