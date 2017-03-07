using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    class RequireRoleAttribute : PreconditionAttribute
    {
        private List<string> _roles = new List<string>();
        public ReadOnlyCollection<string> Roles { get; private set; }

        public CompareMode Mode { get; private set; }
        public RequireRoleAttribute(CompareMode mode, params string[] roles)
        {
            Roles = new ReadOnlyCollection<string>(_roles);

            Mode = mode;
            _roles.AddRange(roles);
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var user = context.User as SocketGuildUser;

            if (user == null) return PreconditionResult.FromError("User is not in the correct role.");

            var matches = 0;

            foreach (var role in Roles)
            {
                if (user.Roles.Any(a => a.Name.Equals(role, StringComparison.OrdinalIgnoreCase)))
                {
                    matches++;

                    if (Mode == CompareMode.Or)
                    {
                        return PreconditionResult.FromSuccess();
                    }
                }
            }
            
            if(matches == Roles.Count)
            {
                return PreconditionResult.FromSuccess();
            }

            await Task.Yield();
            return PreconditionResult.FromError("User is not in the correct roles.");
        }
    }

    public enum CompareMode
    {
        And,
        Or
    }
}
