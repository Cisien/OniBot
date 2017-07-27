using System;
using System.Threading.Tasks;
using Discord.Commands;
using OniBot.CommandConfigs;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OniBot.Infrastructure
{
    class ConfigurationPreconditionAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var logger = (ILogger)map.GetService(typeof(ILogger));
            if (!(context is SocketCommandContext ctx))
            {
                return PreconditionResult.FromError("Command must have a socket context");
            }

            if (!(context.User is SocketGuildUser user))
            {
                return PreconditionResult.FromError("This command must be run from a server channel.");
            }
            var app = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            if (context.User.Id == app.Owner.Id)
            {
                logger.LogWarning($"Command being run by owner: {app.Owner.Username}: {command.Aliases.First()}");
                return PreconditionResult.FromSuccess();
            }

            var config = (PermissionsConfig)map.GetService(typeof(PermissionsConfig));
            config.Reload(context.Guild.Id);

            var roleIds = user.Roles.Select(a => a.Id);
            
            var cmd = command.Aliases[0];
            
            if(config.Permissions.TryGetValue(cmd, out List<ulong> cmdPerms))
            {
                foreach (var roleId in cmdPerms)
                {
                    if (roleIds.Contains(roleId))
                    {
                        logger.LogInformation($"user granted permission to run command: {context.User.Username}: {command.Aliases.First()}");
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
                        logger.LogWarning($"Command being run by wildcard: {context.User.Username}: {command.Aliases.First()}");
                        return PreconditionResult.FromSuccess();
                    }
                }
            }

            logger.LogInformation($"User does not have permission to command: {context.User.Username}: {command.Aliases.First()}");
            return PreconditionResult.FromError("You do not have permission to run this command");
        }

    }
}
