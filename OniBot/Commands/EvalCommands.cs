using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Newtonsoft.Json;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("eval")]
    [Summary("A set of commands for executing C# code")]
    [ConfigurationPrecondition]
    public class EvalCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private static Assembly[] assemblies = new[] {
            typeof(Enumerable).GetTypeInfo().Assembly,
            typeof(ChannelPermissions).GetTypeInfo().Assembly,
            typeof(SocketCommandContext).GetTypeInfo().Assembly,
            typeof(JsonConvert).GetTypeInfo().Assembly,
            typeof(SocketGuildUser).GetTypeInfo().Assembly
        };

        private static string[] namespaces = new[] {
            "System", "System.Linq",
            "System.Diagnostics", "System.Collections",
            "System.Threading.Tasks", "Discord",
            "Discord.Commands", "Newtonsoft.Json",
            "Discord.WebSocket"
        };

        private static ScriptOptions opts = ScriptOptions.Default.AddImports(namespaces).AddReferences(assemblies);

        [Command(RunMode = RunMode.Async)]
        public async Task Evaulate([Remainder]string code)
        {
            var sw = Stopwatch.StartNew();
            bool success = false;
            object result;
            try
            {
                result = await CSharpScript.EvaluateAsync(code, options: opts, globals: Context).ConfigureAwait(false);
                success = true;
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
            embed.AddField(a => a.WithName($"Result: {result?.GetType()?.Name ?? "null"}").WithValue(Format.Code($"{result ?? " "}")));

            await Context.Channel.SendMessageAsync(string.Empty, embed: embed).ConfigureAwait(false);
        }
    }
}
