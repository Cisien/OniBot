using Discord.Commands;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using OniBot.CommandConfigs;
using System.Text;

namespace OniBot.Commands
{
    [Group("randomly")]
    [Summary("A group of commands used for modifying the random channel messages")]
    [ConfigurationPrecondition]
    public class RandomlyCommands : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private readonly RandomlyConfig _config;

        public RandomlyCommands(RandomlyConfig config)
        {
            _config = config;
        }
        
        [Command("show")]
        [Summary("Sends the current running randomly config in a DM")]
        public async Task Show()
        {
            var configTxt = Configuration.GetJson<RandomlyConfig>(_config.ConfigKey, Context.Guild.Id);
            await Context.User.SendFileAsync(Encoding.UTF8.GetBytes(configTxt), "randomly.json").ConfigureAwait(false);
        }

        [Command("min")]
        [Summary("Sets the current minimum range for random messages to appear in.")]
        public async Task Min(
            [Summary("The minimum number of messages to constrain on.")]int min)
        {
            await Configuration.Modify<RandomlyConfig>(_config.ConfigKey, a =>
            {
                a.MinMessages = min;
            }, Context.Guild.Id).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"Maximum number of messages between interjecting: \"{min}\".").ConfigureAwait(false);
        }

        [Command("max")]
        [Summary("Sets the current minimum range for random messages to appear in.")]
        public async Task Max(
            [Summary("The maximum number of messages to constrain on.")]int max)
        {
            await Configuration.Modify<RandomlyConfig>(_config.ConfigKey, a =>
            {
                a.MaxMessages = max;
            }, Context.Guild.Id).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"Maximum number of messages between interjecting: \"{max}\".").ConfigureAwait(false);
        }

        [Command("add")]
        [Summary("Adds a message to display randomly. Can be used in a file upload or with an image URL")]
        public async Task Add(
            [Summary("The message to add. If including a URL to an image, it needs to be the last thing in the message.")]string message)
        {
            await Configuration.Modify<RandomlyConfig>(_config.ConfigKey, a =>
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
            }, Context.Guild.Id).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"\"{message}\" added.").ConfigureAwait(false);
        }

        [Command("remove")]
        [Summary("Removes a message from the list of random messages.")]
        public async Task Remove(
            [Summary("The full message to remove without the image link.")]string message)
        {
            await Configuration.Modify<RandomlyConfig>(_config.ConfigKey, async a =>
            {
                var toRemove = a.RandomMessages.SingleOrDefault(b => b.Message == message);

                if (toRemove == null)
                {
                    await Context.User.SendMessageAsync($"\"{message}\" was not found.").ConfigureAwait(false);
                }
                else
                {
                    a.RandomMessages.Remove(toRemove);
                }
            }, Context.Guild.Id).ConfigureAwait(false);

            await Context.User.SendMessageAsync($"\"{message}\" removed.").ConfigureAwait(false);
        }
    }
}
