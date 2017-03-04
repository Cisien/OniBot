using Discord;
using System.Threading.Tasks;

namespace OniBot.Interfaces
{
    interface IBotBehavior
    {
        string Name { get; }

        Task RunAsync(IDiscordClient client);
    }
}
