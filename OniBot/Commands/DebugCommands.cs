using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("debug")]
    [RequireOwner]
    public class DebugCommands : ModuleBase, IBotCommand
    {
        [Command("config")]
        [Summary("Sends the currently running config")]
        [RequireRole(CompareMode.Or, OniRoles.BotSmith, OniRoles.MasterArchitects)]
        public async Task DumpConfig(
        [Summary("[Optional] If supplied, uploads just the single config to Discord")]string config = null)
        {
            var files = Directory.GetFiles("./config/", "*.json");

            if (string.IsNullOrWhiteSpace(config))
            {
                foreach (var file in files)
                {
                    var contents = File.ReadAllBytes(file);
                    await Context.User.SendFileAsync(contents, Path.GetFileName(file));
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            else
            {
                var file = files.SingleOrDefault(a => a.Contains(config));
                var contents = File.ReadAllBytes(file);
                await Context.User.SendFileAsync(contents, Path.GetFileName(file));
            }
        }

        [Command("bot")]
        [Summary("Gets the current run state of the bot")]
        [RequireOwner]
        public async Task DumpMyself()
        {
            var props = DumpProps(Context.Client.CurrentUser);
            await Context.User.SendMessageAsync(props);

            var cPerms = GetChannelPerms(Context.Client.CurrentUser as IGuildUser);
            await Context.User.SendMessageAsync($"```{"Channel Permissions".PadRight(20)}{string.Join(", ", cPerms)}```");

            var gPerms = GetGuildPerms(Context.Client.CurrentUser as IGuildUser);
            await Context.User.SendMessageAsync($"```{"Guild Permissions".PadRight(20)}{string.Join(", ", gPerms)}```");
        }

        [Command("user")]
        [Summary("Gets the current run state of a user")]
        [RequireOwner]
        public async Task DumpUser([Remainder] string user)
        {
            var users = await Context.Guild.GetUsersAsync();
            var selectedUser = users.SingleOrDefault(a => a.Mention == user || a.Mention == user.Replace("<@", "<@!"));
            if (selectedUser == null)
            {
                return;
            }

            var props = DumpProps(selectedUser);
            await Context.User.SendMessageAsync(props);

            var cPerms = GetChannelPerms(selectedUser);
            await Context.User.SendMessageAsync($"```{"Channel Permissions".PadRight(20)}{string.Join(", ", cPerms)}```");

            var gPerms = GetGuildPerms(selectedUser);
            await Context.User.SendMessageAsync($"```{"Guild Permissions".PadRight(20)}{string.Join(", ", gPerms)}```");
        }

        [Command("chat")]
        [Summary("Gets the current run state of a user")]
        [RequireOwner]
        public async Task DumpChat([Remainder] string count)
        {
            var amount = int.Parse(count);

            var messages = await Context.Channel.GetMessagesAsync(limit: amount, fromMessageId: Context.Message.Id, dir: Direction.Before).ToList();
            var userDmChannel = await Context.User.CreateDMChannelAsync();
            foreach (var messageContainer in messages)
            {
                foreach (var message in messageContainer)
                {
                    var props = DumpProps(message);

                    await userDmChannel.SendMessageAsync(props);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }

        [Command("info")]
        [Summary("Provides basic information about the bot and its environment")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await Context.User.SendMessageAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private string DumpProps(object property)
        {
            var sb = new StringBuilder();
            var userType = property.GetType();
            sb.AppendLine($"```{"Type".PadRight(20)}{userType}");
            var props = userType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in props)
            {

                var key = prop.Name.PadRight(20);
                object value;
                try
                {
                    if (prop.Name == "Roles")
                    {
                        value = string.Join(", ", prop.GetValue(property) as IEnumerable<SocketRole>);
                    }
                    else
                    {
                        value = prop.GetValue(property);
                    }
                }
                catch (Exception ex)
                {
                    value = $"Exception while processing property value: {ex.Message}";
                }
                sb.AppendLine($"{key}{value}");
            }

            sb.Append("```");
            return sb.ToString();
        }

        private List<ChannelPermission> GetChannelPerms(IGuildUser user)
        {
            var hasChannelPerms = new List<ChannelPermission>();
            var channel = Context.Channel as IGuildChannel;
            if (channel != null)
            {
                var userChannelPerms = user.GetPermissions(channel);

                var channelPerms = Enum.GetValues(typeof(ChannelPermission)).Cast<ChannelPermission>();


                foreach (var perm in channelPerms)
                {
                    if (userChannelPerms.Has(perm))
                    {
                        hasChannelPerms.Add(perm);
                    }
                }
            }

            return hasChannelPerms;
        }

        private List<GuildPermission> GetGuildPerms(IGuildUser user)
        {
            var userGuildPerms = user.GuildPermissions;
            var guildPerms = Enum.GetValues(typeof(GuildPermission)).Cast<GuildPermission>();

            var hasGuildPerms = new List<GuildPermission>();

            foreach (var perm in guildPerms)
            {
                if (userGuildPerms.Has(perm))
                {
                    hasGuildPerms.Add(perm);
                }
            }
            return hasGuildPerms;
        }
    }
}
