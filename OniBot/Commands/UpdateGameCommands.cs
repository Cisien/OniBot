using Discord;
using Discord.Commands;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OniBot.Commands
{
    [Group("game")]
    [Summary("A set of commands for managing the list of games the bot is currently playing.")]
    public class UpdateGameCommands : ModuleBase, IBotCommand
    {
        private const string _configKey = "updategame";

        [Command("show")]
        [Summary("Sends you the currently running Game Config")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task Show()
        {
            var config = Configuration.Get<GamesConfig>(_configKey);
            var cfg = JsonConvert.SerializeObject(config, Formatting.Indented);

            await Context.User.SendMessageAsync($"```{cfg}```");
        }

        [Command("remove")]
        [Summary("Removes the game from the bot's list.")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        private async Task Remove(
            [Summary("The game to remove from the bot's list.")]string game)
        {
            await Configuration.Modify<GamesConfig>(_configKey, a =>
            {
                if (a.Games.Contains(game))
                {
                    a.Games.Remove(game);
                }
            });

            await Context.User.SendMessageAsync($"{game} removed.");
        }

        private async Task Add(
        [Summary("The game to add to the bot's list.")]string game)
        {
            await Configuration.Modify<GamesConfig>(_configKey, a =>
            {
                if (!a.Games.Contains(game))
                {
                    a.Games.Add(game);
                }
            });

            await Context.User.SendMessageAsync($"{game} added.");
        }
    }
}
