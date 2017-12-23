using OniBot.Interfaces;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace OniBot.CommandConfigs
{
    public class TagsConfig : CommandConfig
    {
        //<string, string> = <command, response>
        public Dictionary<string, string> Commands { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        [JsonIgnore]
        public override string ConfigKey => "customcommands";
        
    }
}
