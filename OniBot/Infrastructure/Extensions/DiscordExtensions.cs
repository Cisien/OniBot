using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Threading.Tasks;

namespace OniBot
{
    public static class DiscordExtensions
    {
        public static Task<string> GetUserName(this SocketGuildUser user)
        {
            var userName = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;

            return Task.FromResult(userName);
        }

        public static async Task<IUserMessage> SafeReplyAsync(this ModuleBase<SocketCommandContext> module, string message)
        {
            message = message.Replace("@​everyone", "@every\x200Bone").Replace("@here", "@he\x200Bre");
            var response = await module.Context.Channel.SendMessageAsync(message).ConfigureAwait(false);
            return response;
        }

        public static async Task<IUserMessage> SendMessageAsync(this IUser user, string message)
        {
            var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
            return await dmChannel.SendMessageAsync(message).ConfigureAwait(false);
        }

        public static async Task<IUserMessage> SendFileAsync(this IUser user, byte[] file, string filename = null)
        {
            var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);

            using (var ms = new MemoryStream(file))
            {
                ms.Position = 0;
                return await dmChannel.SendFileAsync(ms, filename).ConfigureAwait(false);
            }
        }

        public static async Task<IUserMessage> SendEmbedAsync(this IUser user, Embed embed, string message = null)
        {
            var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);

            return await dmChannel.SendMessageAsync(message, embed: embed).ConfigureAwait(false);
        }

        public static async Task<IUserMessage> SendFileAsync(this IMessageChannel channel, byte[] data, string filename, string message = null)
        {
            using (var ms = new MemoryStream(data))
            {
                ms.Position = 0;
                return await channel.SendFileAsync(ms, filename, message).ConfigureAwait(false);
            }
        }
    }
}
