using Discord.Commands;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("avatar")]
    [Summary("A group of commands related to bot avatar management")]
    [ConfigurationPrecondition]
    public class AvatarCommand : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private const string _configKey = "avatar";
 
        [Command("show")]
        [Summary("Sends you the current running Avatar config.")]
        public async Task Show()
        {
            var cfg = Configuration.GetJson<AvatarConfig>(_configKey);
            await Context.User.SendMessageAsync($"```{cfg}```").ConfigureAwait(false);
        }

        [Command("remove")]
        [Summary("Removes the avatar from the list.")]
        public async Task Remove([Summary("The name of the avatar to remove."), Remainder]string name)
        {
            await Configuration.Modify<AvatarConfig>(_configKey, a =>
            {
                if (a.Avatars.ContainsKey(name))
                {
                    a.Avatars.Remove(name);
                }
            }).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"{name} removed.").ConfigureAwait(false);
        }

        [Command("add")]
        [Summary("Adds the supplied avatar to the list.")]
        public async Task Add(
        [Summary("The name of the avatar to add.")]string name,
        [Summary("The URL of the avatar image.")]string url)
        {
            await Configuration.Modify<AvatarConfig>(_configKey, a =>
            {
                if (Context.Message.Attachments.Count > 0)
                {
                    url = Context.Message.Attachments.FirstOrDefault().Url;
                }

                if (!a.Avatars.ContainsKey(name))
                {
                    a.Avatars.Add(name, url);
                }
            }).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"{name} added.").ConfigureAwait(false);

        }
    }
}
