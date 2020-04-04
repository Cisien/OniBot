using Discord;

namespace OniBot.Interfaces
{
    public interface IBotConfig
    {
        LogSeverity LogLevel { get; set; }
        bool AlwaysDownloadUsers { get; set; }
        int MessageCacheSize { get; set; }
        string Token { get; set; }
        char PrefixChar { get; set; }
        string AzureVoiceKey { get; set; }
    }
}