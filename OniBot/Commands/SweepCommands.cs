using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class SweepCommands : ModuleBase, IBotCommand
    {
        private static Dictionary<string, string> equiped = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Random random = new Random();

        [Command("sweep")]
        [Alias("sw")]
        [Summary("Cleans up the mess in the room")]
        [RequireUserPermission(ChannelPermission.SendMessages)]
        public async Task Attack([Remainder] string target)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }

            var username = await user.GetUserName();
            var hasEquiped = equiped.ContainsKey(username);
            var weapon = hasEquiped ? $" with a {equiped[username]}" : string.Empty;
            await ReplyAsync($"_{username} sweeps up {target}{weapon}._");
        }

        [Command("equip")]
        [Alias("eq")]
        [Summary("Equips a broom to use for sweeping")]
        [RequireUserPermission(ChannelPermission.SendMessages)]
        public async Task Equip([Remainder] string weapon)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }
            var username = await user.GetUserName();

            equiped[username] = weapon;
            await ReplyAsync($"_{username} equips a {weapon}_");
        }

        [Command("unequip")]
        [Alias("ueq")]
        [Summary("Sheathes equiped broom")]
        [RequireUserPermission(ChannelPermission.SendMessages)]
        public async Task Unequip()
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }
            var username = await user.GetUserName();

            equiped.Remove(username);
            await ReplyAsync($"_{username} puts away their cleaning device._");
        }
    }
}
