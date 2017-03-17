using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("perm")]
    [ConfigurationPrecondition]
    public class PermissionCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private PermissionsConfig _config;
        private ICommandHandler _handler;

        public PermissionCommands(PermissionsConfig config, ICommandHandler handler)
        {
            _config = config;
            _handler = handler;
        }

        [Command("add"), Priority(200)]
        public async Task AddPermission(string command, SocketRole role)
        {
            await AddPermission(command, role.Id).ConfigureAwait(false);
        }

        [Command("add"), Priority(100)]
        public async Task AddPermission(string command, ulong roleId)
        {
            command = command.ToLower();

            var role = Context.Guild.Roles.SingleOrDefault(a => a.Id == roleId);
            if (role == null)
            {
                await ReplyAsync("Role not found.").ConfigureAwait(false);
                return;
            }

            var commands = await _handler.BuildHelp(Context).ConfigureAwait(false);
            if (command != "*" && !commands.SelectMany(a => a.Commands).Any(a => a.Alias == command))
            {
                await ReplyAsync("Command either does not exist, or you do not already have permissions to it.").ConfigureAwait(false);
                return;
            }

            await Configuration.Modify<PermissionsConfig>(_config.ConfigKey, async a =>
            {
                if (!a.Permissions.ContainsKey(command))
                {
                    a.Permissions.Add(command, new List<ulong>());
                }

                if (a.Permissions[command].Contains(roleId))
                {
                    await ReplyAsync($"`{role.Name}` is already enabled for {command}").ConfigureAwait(false);
                    return;
                }

                a.Permissions[command].Add(role.Id);

                await ReplyAsync($"{command} enabled for `{role.Name}`").ConfigureAwait(false);
            }, Context.Guild.Id).ConfigureAwait(false);
        }

        [Command("remove"), Priority(200)]
        public async Task RemovePermission(string command, SocketRole role)
        {
            await RemovePermission(command, role.Id).ConfigureAwait(false);
        }

        [Command("remove"), Priority(100)]
        public async Task RemovePermission(string command, ulong roleId)
        {
            command = command.ToLower();

            var role = Context.Guild.Roles.SingleOrDefault(a => a.Id == roleId);
            if (role == null)
            {
                await ReplyAsync("Role not found.").ConfigureAwait(false);
                return;
            }

            await Configuration.Modify<PermissionsConfig>(_config.ConfigKey, async a =>
            {
                if (!a.Permissions.ContainsKey(command))
                {
                    a.Permissions.Add(command, new List<ulong>());
                }

                a.Permissions[command].Remove(role.Id);

                await ReplyAsync($"{command} disabled for `{role.Name}`").ConfigureAwait(false);
            }, Context.Guild.Id).ConfigureAwait(false);
        }

        [Command("roles")]
        public async Task GetRoles()
        {
            var roles = Context.Guild.Roles.Select(a => new { Id = a.Id, Name = a.Name });
            var json = JsonConvert.SerializeObject(roles, Formatting.Indented);
            await Context.User.SendMessageAsync($"```{json}```").ConfigureAwait(false);
        }

        [Command("show"), Priority(300)]
        public async Task ShowPermissions()
        {
            var json = Configuration.GetJson<PermissionsConfig>(_config.ConfigKey, Context.Guild.Id);
            await Context.User.SendFileAsync(Encoding.UTF8.GetBytes(json), "permissions.json").ConfigureAwait(false);
        }

        [Command("show"), Priority(200)]
        public async Task ShowPermission(string command)
        {
            command = command.ToLower();
            _config.Reload(Context.Guild.Id);
            if (!_config.Permissions.ContainsKey(command))
            {
                await ReplyAsync("Command not found").ConfigureAwait(false);
                return;
            }

            var json = JsonConvert.SerializeObject(_config.Permissions[command].Select(a => Context.Guild.Roles.Single(b => b.Id == a).Name));
            await Context.User.SendFileAsync(Encoding.UTF8.GetBytes(json), "permissions.json").ConfigureAwait(false);
        }

        [Command("show"), Priority(100)]
        public async Task ShowPermission(SocketRole role)
        {
            await ShowPermission(role.Id).ConfigureAwait(false);
        }

        [Command("show"), Priority(50)]
        public async Task ShowPermission(ulong roleId)
        {
            var role = Context.Guild.Roles.SingleOrDefault(a => a.Id == roleId);
            if (role == null)
            {
                await ReplyAsync("Role not found.").ConfigureAwait(false);
                return;
            }

            _config.Reload(Context.Guild.Id);

            var commands = new List<string>();

            foreach (var perm in _config.Permissions)
            {
                if (perm.Value.Contains(role.Id))
                {
                    commands.Add(perm.Key);
                }
            }

            var json = JsonConvert.SerializeObject(commands);
            await Context.User.SendFileAsync(Encoding.UTF8.GetBytes(json), "permissions.json").ConfigureAwait(false);
        }
    }
}
