using Discord.Commands;
using Discord.WebSocket;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Threading.Tasks;
using OniBot.CommandConfigs;
namespace OniBot.Commands
{
    [ConfigurationPrecondition]
    public class SweepCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        public SweepCommands(SweepConfig config)
        {
            _config = config;
        }

        private static readonly Random random = new Random();
        private SweepConfig _config;

        [Command("sweep")]
        [Summary("Cleans up the mess in the room")]
        public async Task Sweep(
            [Summary("The person or thing to sweep up")][Remainder] string target)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }
            _config.Reload(Context.Guild.Id);
            var username = await user.GetUserName().ConfigureAwait(false);
            var hasEquiped = _config.Equiped.ContainsKey(user.Id);
            var weapon = hasEquiped ? $" with a {_config.Equiped[user.Id]}" : string.Empty;

            await this.SafeReplyAsync($"_{username} sweeps up {target}{weapon}._").ConfigureAwait(false);
        }

        [Command("equip")]
        [Summary("Equips a broom to use for sweeping")]
        public async Task Equip(
            [Summary("The person or thing to equip as a broom")][Remainder] string weapon)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }

            var username = await user.GetUserName().ConfigureAwait(false);
            await Configuration.Modify<SweepConfig>(_config.ConfigKey, a =>
            {
                a.Equiped[user.Id] = weapon;
            }, Context.Guild.Id).ConfigureAwait(false);

            await this.SafeReplyAsync($"_{username} equips a {weapon}_").ConfigureAwait(false);
        }

        [Command("unequip")]
        [Summary("Sheathes the equiped broom")]
        public async Task Unequip()
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }

            var username = await user.GetUserName().ConfigureAwait(false);
            await Configuration.Modify<SweepConfig>(_config.ConfigKey, a =>
            {
                a.Equiped.Remove(user.Id);
            }, Context.Guild.Id).ConfigureAwait(false);

            await this.SafeReplyAsync($"_{username} puts away their cleaning device._").ConfigureAwait(false);
        }
    }
}
