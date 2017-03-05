using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class BlameCommand : ModuleBase, IBotCommand
    {
        [Command("blame")]
        [Summary("Blames someone or something")]
        [RequireUserPermission(ChannelPermission.SendMessages)]
        public async Task Blame(
            [Summary("The user or random thing to blame")]string toBlame, 
            [Summary("[Optional] The reason for the blaming")][Remainder] string because = null)
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
