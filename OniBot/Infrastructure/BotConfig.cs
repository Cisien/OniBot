using System;
using Discord;

namespace OniBot.Interfaces
{
    public class BotConfig : IBotConfig
    {
        public string[] Games { get; set; } = new string[] { "OxygenNotIncluded" };
        public LogSeverity LogLevel { get; set; }
        public bool AlwaysDownloadUsers { get; set; }
        public int MessageCacheSize { get; set; }
        public string Token { get; set; }
        public char PrefixChar { get; set; }
 
    }
}