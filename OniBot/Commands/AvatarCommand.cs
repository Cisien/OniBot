using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class AvatarCommand : ModuleBase, IBotCommand
    {
        [Command("avatar")]
        [Summary("Provides commands for modifying the list of avatars the bot will pick from each day")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task Avatar(
            [Summary("The command to run. Must be one of add|remove|show")]string command,
            [Summary("The name of the avatar")]string name = null,
            [Summary("[Optional] The URL of an image that will be added to the image rotation. If empty, this command must be run in a file updload")][Remainder]string url = null)
        {
            var config = Configuration.Get<AvatarConfig>("avatar");
            if (config.Avatars == null)
            {
                config.Avatars = new Dictionary<string, string>();
            }
            
            switch (command)
            {
                case "add":
                    await DoAdd(config, name, url);
                    break;
                case "remove":
                    await DoRemove(config, name);
                    break;
                case "show":
                    await DoShow(config);
                    break;
                default:
                    await DoDefault();
                    break;
            }

            Configuration.Write(config, "avatar");
        }

        private async Task DoDefault()
        {
            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync("Unknown Command. See !help avatar");
        }

        private async Task DoShow(AvatarConfig config)
        {
            var cfg = JsonConvert.SerializeObject(config, Formatting.Indented);

            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"```{cfg}```");
        }

        private async Task DoRemove(AvatarConfig config, string name)
        {
            if (config.Avatars.ContainsKey(name))
            {
                config.Avatars.Remove(name);
            }

            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"{name} removed.");

        }

        private async Task DoAdd(AvatarConfig config, string name, string url)
        {
            if (Context.Message.Attachments.Count > 0)
            {
                url = Context.Message.Attachments.FirstOrDefault().Url;
            }

            if (!config.Avatars.ContainsKey(name))
            {
                config.Avatars.Add(name, url);
            }

            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"{name} added.");

        }
    }
}
