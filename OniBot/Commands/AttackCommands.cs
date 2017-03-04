using Discord.Commands;
using Discord.WebSocket;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    public class AttackCommands : ModuleBase, IBotCommand
    {
        private static Dictionary<string, string> equiped = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, int> modifier = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly Random random = new Random();

        [Command("attack", RunMode = RunMode.Async)]
        [Alias("at")]
        [Summary("Attacks someone")]
        public async Task Attack([Remainder] string target)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }

            var username = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;
            var hasEquiped = equiped.ContainsKey(username);
            var weapon = hasEquiped ? $"with {equiped[username]} " : string.Empty;
            int dmgModifier = hasEquiped ? modifier[username] : 50;
            string damage = random.Next(0, dmgModifier).ToString("N0");
            await ReplyAsync($"_{username} attacks {target} {weapon}for {damage} damage._");
        }

        [Command("equip", RunMode = RunMode.Async)]
        [Alias("eq")]
        [Summary("Equips a weapon to use for attacking")]
        public async Task Equip([Remainder] string weapon)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }
            var username = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;

            equiped[username] = weapon;
            modifier[username] = random.Next(50, int.MaxValue);
            await ReplyAsync($"_{username} equips {weapon}_");
        }

        [Command("unequip", RunMode = RunMode.Async)]
        [Alias("ueq")]
        [Summary("Sheathes equiped weapon")]
        public async Task Unequip()
        {
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }
            var username = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname;

            equiped.Remove(username);
            modifier.Remove(username);
            await ReplyAsync($"_{username} puts away their weapon_");
        }
    }
}
