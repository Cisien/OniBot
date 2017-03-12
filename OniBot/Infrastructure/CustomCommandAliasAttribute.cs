using Discord.Commands;
using OniBot.CommandConfigs;
using System.Linq;

namespace OniBot.Infrastructure
{    public class CustomCommandAliasAttribute : AliasAttribute
    {
        public CustomCommandAliasAttribute(params string[] aliases) : base(GetDynamicCommands(aliases[0]))
        {
        }

        private static string[] GetDynamicCommands(string text = null)
        {
            var config = new CustomCommandsConfig();

            if(config?.Commands?.Count == 0) {
                return new[] { "teapot" };
            }

            return config.Commands.Select(a => a.Key).ToArray();
        }
    }
}
