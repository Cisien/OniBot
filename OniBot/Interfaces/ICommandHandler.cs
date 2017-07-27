using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using OniBot.Infrastructure.Help;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OniBot.Interfaces
{
    public interface ICommandHandler
    {
        Task InstallAsync(IServiceCollection map);
        Task ReloadCommands();
        Task<List<Help>> BuildHelpAsync(ICommandContext context);
    }
}