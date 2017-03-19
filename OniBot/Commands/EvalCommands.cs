using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
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
        private IDependencyMap _map;

        public EvalCommands(IDependencyMap map) {
            _map = map;
        }

        [Command]
        public async Task Evaulate([Remainder]string code)
        {
            var sw = Stopwatch.StartNew();
            bool success = false;
            object result;
            try
            {
                if(Context.User is SocketGuildUser guildUser)
                    guildUser.GetAvatarUrl();

                var linqAssembly = typeof(Enumerable).GetTypeInfo().Assembly;
                var discordAssembly = typeof(ChannelPermissions).GetTypeInfo().Assembly;
                var discordCommandAssembly = typeof(SocketCommandContext).GetTypeInfo().Assembly;
                
                var opts = ScriptOptions.Default.AddImports("System", "System.Linq", "System.Diagnostics", "System.Collections", "System.Threading.Tasks", "Discord", "Discord.Commands")
                                                .AddReferences(linqAssembly, discordAssembly, discordCommandAssembly);

                result = await CSharpScript.EvaluateAsync(code,options: opts, globals: Context, globalsType: typeof(SocketCommandContext)).ConfigureAwait(false);
                success = true;
            }
            catch (Exception ex)
            {
                result = $"Unable to evaluate: {ex.Message}";
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

        public class Globals
        {
            public SocketCommandContext Discord { get; set; }
        }
    }
}
