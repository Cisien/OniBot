using Discord;
using Discord.Commands;
using OniBot.Infrastructure.Help;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OniBot.Interfaces
{
    public interface ICommandHandler
    {
        Task InstallAsync(IDependencyMap map);
        Task ReloadCommands();
        Task<List<Help>> BuildHelp(ICommandContext context);
    }
}