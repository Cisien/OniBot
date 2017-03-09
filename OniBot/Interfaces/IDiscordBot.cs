using System;
using System.Threading.Tasks;

namespace OniBot
{
    internal interface IDiscordBot : IDisposable
    {
        Task RunBotAsync();
    }
}