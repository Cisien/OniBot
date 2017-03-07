using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("avatar")]
    [Summary("A group of commands related to bot avatar management")]
    public class AvatarCommand : ModuleBase, IBotCommand
    {
        private const string _configKey = "avatar";
 
        [Command("show")]
        [Summary("Sends you the current running Avatar config")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Show()
        {
            var config = Configuration.Get<AvatarConfig>(_configKey);
            var cfg = JsonConvert.SerializeObject(config, Formatting.Indented);

            await Context.User.SendMessageAsync($"```{cfg}```");
        }

        [Command("remove")]
        [Summary("Sends you the current running Avatar config")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Remove([Summary("The name of the avatar to remove.")]string name)
        {
            await Configuration.Modify<AvatarConfig>(_configKey, a =>
            {
                if (a.Avatars.ContainsKey(name))
                {
                    a.Avatars.Remove(name);
                }
            });

            await Context.User.SendMessageAsync($"{name} removed.");
        }

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
            });

            await Context.User.SendMessageAsync($"{name} added.");

        }
    }
}
