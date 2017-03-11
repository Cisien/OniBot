using Discord.Commands;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("echo")]
    [Summary("Repeats the text that follows")]
    public class EchoCommand : ModuleBase<SocketCommandContext>, IBotCommand
    {
        [Command]
        [RequireOwner]
        public async Task Echo([Remainder]string message)
        {
            await ReplyAsync(message);
        }
    }
}
