using Discord.Commands;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("echo")]
    [Summary("Repeats the text that follows")]
    [ConfigurationPrecondition]
    public class EchoCommand : ModuleBase<SocketCommandContext>, IBotCommand
    {
        [Command]
        public async Task Echo([Remainder]string message)
        {
            await this.SafeReplyAsync(message).ConfigureAwait(false);
        }
    }
}
