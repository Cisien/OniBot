using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    public static class DiscordExtensions
    {
        public static Task<string> GetUserName(this Discord.WebSocket.SocketGuildUser user)
        {
            var userName = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;

            return Task.FromResult(userName);
        }
    }
}
