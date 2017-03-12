using System;
using System.Threading.Tasks;
using Discord.Commands;
using OniBot.CommandConfigs;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;

namespace OniBot.Infrastructure
{
    class ConfigurationPreconditionAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if (!(context is SocketCommandContext ctx))
            {
                return PreconditionResult.FromError("Command must have a socket context");
            }

            if (!(context.User is SocketGuildUser user))
            {
                return PreconditionResult.FromError("This command must be run from a server channel.");
            }
            var app = await context.Client.GetApplicationInfoAsync();

            if (context.User.Id == app.Owner.Id)
            {
                return PreconditionResult.FromSuccess();
            }

            var config = map.Get<PermissionsConfig>();
            config.Reload(context.Guild.Id);

            var roleIds = user.Roles.Select(a => a.Id);
            
            var cmd = command.Aliases[0];
            
            if(config.Permissions.TryGetValue(cmd, out List<ulong> cmdPerms))
            {
                foreach (var roleId in cmdPerms)
                {
                    if (roleIds.Contains(roleId))
                    {
                        return PreconditionResult.FromSuccess();
                    }
                }
            }

            if (config.Permissions.TryGetValue("*", out List<ulong> wildcard))
            {
                foreach (var roleId in config.Permissions["*"])
                {
                    if (roleIds.Contains(roleId))
                    {
                        return PreconditionResult.FromSuccess();
                    }
                }
            }
            
            return PreconditionResult.FromError("You do not have permission to run this command");
        }
    }
}
