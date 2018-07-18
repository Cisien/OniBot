using Discord.Commands;
using OniBot.Infrastructure.Help;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OniBot.Interfaces
{
    public interface ICommandHandler
    {
        Task InstallAsync();
        Task ReloadCommands();
        Task<List<Help>> BuildHelpAsync(ICommandContext context);
    }
}