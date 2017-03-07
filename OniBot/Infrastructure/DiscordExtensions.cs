using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    public static class DiscordExtensions
    {
        public static Task<string> GetUserName(this SocketGuildUser user)
        {
            var userName = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;

            return Task.FromResult(userName);
        }

        public static async Task<IUserMessage> SafeReplyAsync(this ModuleBase module, ICommandContext context, string message)
        {
            //todo: get mentions in message, use mention to find user, replace mention with user

            var response = await context.Channel.SendMessageAsync(message);
            return response;
        }

        public static async Task<IUserMessage> SendMessageAsync(this IUser user, string message)
        {
            var dmChannel = await user.CreateDMChannelAsync();
            return await dmChannel.SendMessageAsync(message);
        }

        public static async Task<IUserMessage> SendFileAsync(this IUser user, byte[] file, string message = null)
        {
            var dmChannel = await user.CreateDMChannelAsync();

            using (var ms = new MemoryStream(file))
            {
                ms.Position = 0;
                return await dmChannel.SendFileAsync(ms, message);
            }
        }

        public static async Task<IUserMessage> SendEmbedAsync(this IUser user, Embed embed, string message = null)
        {
            var dmChannel = await user.CreateDMChannelAsync();

            return await dmChannel.SendMessageAsync(message, embed: embed);
        }
    }
}
