using System;
using System.Threading.Tasks;

namespace OniBot
{
    public interface IDiscordBot : IDisposable
    {
        Task RunBotAsync();
    }
}