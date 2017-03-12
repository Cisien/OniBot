using Discord.Commands;
using Discord.WebSocket;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Threading.Tasks;
using OniBot.CommandConfigs;
using System.Collections.Generic;

namespace OniBot.Commands
{
    [ConfigurationPrecondition]
    public class SweepCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private static readonly Random random = new Random();

        [Command("sweep")]
        [Summary("Cleans up the mess in the room")]
        public async Task Attack(
            [Summary("The person or thing to sweep up")][Remainder] string target)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }
            
            var config = Configuration.Get<SweepConfig>("sweep");
            if (config.Equiped == null) config.Equiped = new Dictionary<ulong, string>();

            var username = await user.GetUserName();
            var hasEquiped = config.Equiped.ContainsKey(user.Id);
            var weapon = hasEquiped ? $" with a {config.Equiped[user.Id]}" : string.Empty;
            await ReplyAsync($"_{username} sweeps up {target}{weapon}._");
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

            var config = Configuration.Get<SweepConfig>("sweep");
            if (config.Equiped == null) config.Equiped = new Dictionary<ulong, string>();

            var username = await user.GetUserName();
            config.Equiped[user.Id] = weapon;
            Configuration.Write(config, "sweep");
            await ReplyAsync($"_{username} equips a {weapon}_");
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
            
            var config = Configuration.Get<SweepConfig>("sweep");
            if (config.Equiped == null) config.Equiped = new Dictionary<ulong, string>();

            var username = await user.GetUserName();
            config.Equiped.Remove(user.Id);
            Configuration.Write(config, "sweep");
            await ReplyAsync($"_{username} puts away their cleaning device._");
        }
    }
}
