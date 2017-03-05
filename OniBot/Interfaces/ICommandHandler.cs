using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace OniBot.Interfaces
{
    public interface ICommandHandler
    {
        Task InstallAsync(IDependencyMap map);
        Task ReloadCommands();
        Task<string> PrintCommands(ICommandContext context);
    }
}