using Discord;
using Discord.Commands;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("eval")]
    [Summary("A set of commands for executing C# code")]
    [ConfigurationPrecondition]
    public class EvalCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        [Command(RunMode = RunMode.Async)]
        public async Task Evaulate([Remainder]string code)
        {
            var sw = Stopwatch.StartNew();
            bool success = false;
            object result;
            try
            {
                result = new object();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            sw.Stop();

            var embed = new EmbedBuilder()
                .WithTitle("Eval Result")
                .WithDescription(success ? "Successful" : "Failed")
                .WithColor(success ? new Color(0, 255, 0) : new Color(255, 0, 0))
                .WithAuthor(a => a.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl()).WithName(Context.Client.CurrentUser.Username))
                .WithFooter(a => a.WithText($"{sw.ElapsedMilliseconds}ms"));

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(code, "cs")));
            embed.AddField(a => a.WithName($"Result: {result?.GetType()?.Name ?? "null"}").WithValue(Format.Code($"{result ?? " "}", "txt")));
            
            await Context.Channel.SendMessageAsync(string.Empty, embed: embed).ConfigureAwait(false);
        }
    }
}
