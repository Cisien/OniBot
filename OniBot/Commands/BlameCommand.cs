using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class BlameCommand : ModuleBase, IBotCommand
    {
        [Command("blame", RunMode = RunMode.Async)]
        [Summary("Blames someone")]
        public async Task Blame(string toBlame, [Remainder] string because)
        {
            var user = Context.User as SocketGuildUser;

            if (user == null)
            {
                return;
            }

            var username = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;

            await ReplyAsync($"{username} blames {toBlame} {because}");
        }
    }
}
