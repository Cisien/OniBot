using Discord;
using System.Threading.Tasks;

namespace OniBot.Interfaces
{
    public interface IBotBehavior
    {
        string Name { get; }

        Task RunAsync();
    }
}
