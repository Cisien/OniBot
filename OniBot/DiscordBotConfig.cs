using System;
using Discord;
using OniBot.Interfaces;

namespace OniBot
{
    public class DiscordBotConfig : IDiscordBotConfig
    {
        public string[] Games { get; set; } = new string[] { "OxygenNotIncluded" };
        public LogSeverity LogLevel { get; set; }
        public bool AlwaysDownloadUsers { get; set; }
        public int MessageCacheSize { get; set; }
        public string Token { get; set; }
        public char PrefixChar { get; set; }
    }
}