using System.Collections.Generic;

namespace OniBot.Infrastructure.Help
{
    public class Command
    {
        public string Alias { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public string Summary { get; set; }
    }
}