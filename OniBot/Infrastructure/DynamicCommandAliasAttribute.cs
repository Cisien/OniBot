using Discord.Commands;
using OniBot.CommandConfigs;
using System.Linq;

namespace OniBot.Infrastructure
{    public class DynamcCommandAliasAttribute : AliasAttribute
    {
        public DynamcCommandAliasAttribute(params string[] aliases) : base(GetDynamicCommands(aliases[0]))
        {
        }

        private static string[] GetDynamicCommands(string text = null)
        {
            var config = Configuration.Get<CustomCommandsConfig>(text ?? "customcommands");
            
            if(config?.Commands?.Count == 0) {
                return new[] { "teapot" };
            }

            return config.Commands.Select(a => a.Key).ToArray();
        }
    }
}
