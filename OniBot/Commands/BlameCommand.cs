using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("blame")]
    [Summary("Blames someone or something")]
    public class BlameCommand : ModuleBase<SocketCommandContext>, IBotCommand
    {
        [Command]
        [RequireUserPermission(ChannelPermission.SendMessages)]
        public async Task Blame(
            [Summary("The user or random thing to blame")]string toBlame, 
            [Summary("[Optional] The reason for the blaming"), Remainder] string because = null)
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
