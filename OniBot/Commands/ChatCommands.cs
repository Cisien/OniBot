using Discord;
using Discord.Commands;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("chat")]
    [Summary("Commands to control the chatbot feature")]
    [ConfigurationPrecondition]
    public class ChatCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private ChatConfig _config;

        public ChatCommands(ChatConfig config)
        {
            _config = config;
        }

        [Command("add")]
        [Summary("Adds a channel to the allowed list")]
        public async Task AddChannel([Summary("The channel mention or id to add")]ITextChannel channel)
        {
            await Configuration.Modify<ChatConfig>(_config.ConfigKey, async a =>
            {
                if (a.AllowedChannels.Contains(channel.Id))
                {
                    await ReplyAsync($"Channel {channel.Name} is already allowed");
                    return;
                }

                a.AllowedChannels.Add(channel.Id);
                await ReplyAsync($"{channel.Name} added");
            }, Context.Guild.Id);
        }


        [Command("remove")]
        [Summary("Adds a channel to the allowed list")]
        public async Task RemoveChannel([Summary("The channel mention or id to add")]ITextChannel channel)
        {
            await Configuration.Modify<ChatConfig>(_config.ConfigKey, async a =>
            {
                if (!a.AllowedChannels.Contains(channel.Id))
                {
                    return;
                }

                a.AllowedChannels.Remove(channel.Id);
                await ReplyAsync($"{channel.Name} removed");
            }, Context.Guild.Id);
        }


        [Command("show")]
        [Summary("Adds a channel to the allowed list")]
        public async Task ShowChannel([Summary("The channel mention or id to add")]ITextChannel channel)
        {
            var response = Configuration.GetJson<ChatConfig>(_config.ConfigKey, Context.Guild.Id);
            await Context.User.SendMessageAsync(response);
        }
    }
}
