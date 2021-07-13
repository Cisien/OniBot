using Discord;
using System.IO;
using System.Threading.Tasks;

namespace OniBot
{
    public static class DiscordExtensions
    {
        public static async Task<IUserMessage> SendMessageAsync(this IUser user, string message)
        {
            var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
            return await dmChannel.SendMessageAsync(message).ConfigureAwait(false);
        }

        public static async Task<IUserMessage> SendFileAsync(this IUser user, byte[] file, string filename = null)
        {
            var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);

            using var ms = new MemoryStream(file);
            ms.Position = 0;
            return await dmChannel.SendFileAsync(ms, filename).ConfigureAwait(false);
        }
    }
}
