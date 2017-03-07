using OniBot.Interfaces;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using OniBot.Infrastructure;
using System.Runtime.InteropServices;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Linq;

namespace OniBot.Commands
{
    public class HelpCommand : ModuleBase, IBotCommand
    {
        private ICommandHandler _commandHandler;

        public HelpCommand(ICommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [Command("help")]
        [Summary("Prints the command's help message")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task Help(
            [Summary("[Optional] The name of the command to view the help of.")]string command = null)
        {
            var helpText = await _commandHandler.PrintCommands(Context, command);
            await Context.User.SendMessageAsync($"```{helpText}```");
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
    }
}
