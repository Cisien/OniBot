using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using OniBot.CommandConfigs;

namespace OniBot.Commands
{ 
    [Group("randomly")]
    [Summary("A group of commands used for modifying the random channel messages")]
    public class RandomlyCommands : ModuleBase, IBotCommand
    {
        private const string _configKey = "randomly";

        [Command("show")]
        [Summary("Sends the current running randomly config in a DM")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Show()
        {
            var config = Configuration.Get<RandomlyConfig>(_configKey);
            var configTxt = JsonConvert.SerializeObject(config, Formatting.Indented);
            await Context.User.SendMessageAsync($"```{configTxt}```");            
        }

        [Command("min")]
        [Summary("Sets the current minimum range for random messages to appear in.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Min(
            [Summary("The minimum number of messages to constrain on.")]int min)
        {
            await Configuration.Modify<RandomlyConfig>(_configKey, a =>
            {
                a.MinMessages = min;
            });

            await Context.User.SendMessageAsync($"Maximum number of messages between interjecting: \"{min}\".");
        }

        [Command("max")]
        [Summary("Sets the current minimum range for random messages to appear in.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Max(
            [Summary("The maximum number of messages to constrain on.")]int max)
        {
            await Configuration.Modify<RandomlyConfig>(_configKey, a =>
            {
                a.MaxMessages = max;
            });

            await Context.User.SendMessageAsync($"Maximum number of messages between interjecting: \"{max}\".");
        }

        [Command("add")]
        [Summary("Adds a message to display randomly. Can be used in a file upload or with an image URL")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Add(
            [Summary("The message to add. If including a URL to an image, it needs to be the last thing in the message.")]string message)
        {
            await Configuration.Modify<RandomlyConfig>(_configKey, a =>
            {
                var newMessage = new ImageMessage()
                {
                    Message = message,
                    Image = null
                };

                if (Context.Message.Attachments.Any())
                {
                    newMessage.Image = Context.Message.Attachments.FirstOrDefault()?.Url;
                }
                else if (message.Contains("http"))
                {
                    var startIndex = message.IndexOf("http");
                    var url = message.Substring(startIndex, message.Length - startIndex).Trim();
                    var msg = message.Substring(0, startIndex).Trim();
                    newMessage.Image = url;
                    newMessage.Message = msg;
                }
                var tags = Context.Message.Tags;
            });
            
            await Context.User.SendMessageAsync($"\"{message}\" added.");
        }

        [Command("remove")]
        [Summary("Removes a message from the list of random messages.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Remove(
            [Summary("The full message to remove without the image link.")]string message)
        {
            await Configuration.Modify<RandomlyConfig>(_configKey, async a => {
                var toRemove = a.RandomMessages.SingleOrDefault(b => b.Message == message);

                if (toRemove == null)
                {
                    var dmChannel = await Context.User.CreateDMChannelAsync();
                    await dmChannel.SendMessageAsync($"\"{message}\" was not found.");
                }
                else
                {
                    a.RandomMessages.Remove(toRemove);
                }
            });

            await Context.User.SendMessageAsync($"\"{message}\" removed.");            
        }
    }
}
