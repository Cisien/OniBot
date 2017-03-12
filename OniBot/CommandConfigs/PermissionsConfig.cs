using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class PermissionsConfig : CommandConfig
    {
        [JsonIgnore]
        public override string ConfigKey => nameof(PermissionsConfig);

        //command, [role ids]
        public Dictionary<string, List<ulong>> Permissions { get; set; } = new Dictionary<string, List<ulong>>();
    }
}
