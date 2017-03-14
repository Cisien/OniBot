using Discord.Commands;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("eval")]
    [Summary("A set of commands for executing C# code")]
    [ConfigurationPrecondition]
    public class EvalCommands : ModuleBase, IBotCommand
    {

        public async Task Evaulate([Remainder]string code)
        {

        }
    }
}
