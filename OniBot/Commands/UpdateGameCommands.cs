using Discord;
using Discord.Commands;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;

namespace OniBot.Commands
{
    public class UpdateGameCommands : ModuleBase, IBotCommand
    {
        [Command("updategame")]
        [Summary("Modifies the game list for the bot.")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task UpdateGame(
            [Summary("The command to run. One of add|remove|show")]string command,
            [Summary("The game to add or remove")][Remainder]string game = null)
        {
            var config = Configuration.Get<GamesConfig>("updategame");
            command = command.ToLower();
            switch(command) {
                case "add":
                    await DoAdd(game, config);
                    break;
                case "remove":
                    await DoRemove(game, config);
                    break;
                case "show":
                    await DoShow(config);
                    break;
                default:
                    await DeDefault(config);
                    break;
            }
            Configuration.Write(config, "updategame");
        }

        private async Task DoShow(GamesConfig config)
        {
            var cfg = JsonConvert.SerializeObject(config, Formatting.Indented);

            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"```{cfg}```");
        }

        private async Task DeDefault(GamesConfig config)
        {
            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync("Unknown Command. See !help updategames");
        }

        private async Task DoRemove(string game, GamesConfig config)
        {
            if(config.Games.Contains(game)) {
                config.Games.Remove(game);
            }

            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"{game} removed.");
        }

        private async Task DoAdd(string game, GamesConfig config)
        {
            if (!config.Games.Contains(game)) {
                config.Games.Add(game);
            }

            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"{game} added.");
        }
    }
}
