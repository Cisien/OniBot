using Discord;
using Discord.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
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
        private static Assembly linqAssembly = typeof(Enumerable).GetTypeInfo().Assembly;
        private static Assembly discordAssembly = typeof(ChannelPermissions).GetTypeInfo().Assembly;
        private static Assembly discordCommandAssembly = typeof(SocketCommandContext).GetTypeInfo().Assembly;
        private static Assembly jsonNetAssembly = typeof(JsonConvert).GetTypeInfo().Assembly;

        private static ScriptOptions opts = ScriptOptions.Default.AddImports("System", "System.Linq", "System.Diagnostics", "System.Collections", "System.Threading.Tasks", "Discord", "Discord.Commands", "Newtonsoft.Json")
                                             .AddReferences(linqAssembly, discordAssembly, discordCommandAssembly, jsonNetAssembly);
                                             
        [Command(RunMode = RunMode.Async)]
        public async Task Evaulate([Remainder]string code)
        {
            var sw = Stopwatch.StartNew();
            bool success = false;
            object result;
            try
            {
                result = await CSharpScript.EvaluateAsync(code, options: opts, globals: Context, globalsType: typeof(SocketCommandContext)).ConfigureAwait(false);
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
                
            embed.AddField(a => a.WithName("Code").WithValue($"```cs\n{code}```"));
            embed.AddField(a => a.WithName($"Result: {result?.GetType()?.Name?? "null"}").WithValue($"```{result ?? " "}```"));
            
            await Context.Channel.SendMessageAsync(string.Empty, embed: embed).ConfigureAwait(false);
        }
    }
}
