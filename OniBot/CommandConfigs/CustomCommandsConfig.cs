using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class CustomCommandsConfig
    {
        //<string, string> = <command, response>
        public Dictionary<string, string> Commands { get; set; } = new Dictionary<string, string>();
    }
}
