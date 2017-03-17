using Discord.Commands;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("game")]
    [Summary("A set of commands for managing the list of games the bot is currently playing.")]
    [ConfigurationPrecondition]
    public class UpdateGameCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private const string _configKey = "updategame";

        [Command("show")]
        [Summary("Sends you the currently running Game Config")]
        public async Task Show()
        {
            var cfg = Configuration.GetJson<GamesConfig>(_configKey);
            await Context.User.SendMessageAsync($"```{cfg}```").ConfigureAwait(false);
        }

        [Command("remove")]
        [Summary("Removes the game from the bot's list.")]
        private async Task Remove(
            [Summary("The game to remove from the bot's list.")]string game)
        {
            await Configuration.Modify<GamesConfig>(_configKey, a =>
            {
                if (a.Games.Contains(game))
                {
                    a.Games.Remove(game);
                }
            }).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"{game} removed.").ConfigureAwait(false);
        }

        [Command("add")]
        [Summary("Adds a game to the list the bot will randomly pick from.")]
        private async Task Add(
        [Summary("The game to add to the bot's list.")]string game)
        {
            await Configuration.Modify<GamesConfig>(_configKey, a =>
            {
                if (!a.Games.Contains(game))
                {
                    a.Games.Add(game);
                }
            }).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"{game} added.").ConfigureAwait(false);
        }
    }
}
