using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace OniBot.Commands
{
    public class AdminCommands : ModuleBase, IBotCommand
    {
        [Command("randomly")]
        [Summary("Configures the random message behavior")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task Randomly(string command, [Remainder]string message = null)
        {
            command = command.ToLower();

            var config = Configuration.Get<RandomlyConfig>("randomly");
            if (config.RandomMessages == null)
            {
                config.RandomMessages = new List<ImageMessage>();
            }

            switch (command)
            {
                case "add":
                    await DoAdd(message, config);
                    break;
                case "remove":
                    await DoRemove(message, config);
                    break;
                case "min":
                    await DoMin(message, config);
                    break;
                case "max":
                    await DoMax(message, config);
                    break;
                case "show":
                    await DoShow(config);
                    break;
                default:
                    await DoDefault();
                    break;
            }

            Configuration.Write(config, "randomly");

        }

        private async Task DoDefault()
        {
            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync("Usage: add message [image url]|remove message|min number|max number|show");
        }

        private async Task DoShow(RandomlyConfig config)
        {
            var configTxt = JsonConvert.SerializeObject(config, Formatting.Indented);
            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"```{configTxt}```");
        }

        private async Task DoMin(string message, RandomlyConfig config)
        {
            var dmChannel = await Context.User.CreateDMChannelAsync();
            if (!int.TryParse(message, out int min))
            {
                await dmChannel.SendMessageAsync($"Unable to set min: \"{message}\" is not a valid number.");
            }

            config.MinMessages = min;
            await dmChannel.SendMessageAsync($"Maximum number of messages between interjecting: \"{min}\".");
        }

        private async Task DoMax(string message, RandomlyConfig config)
        {
            var dmChannel = await Context.User.CreateDMChannelAsync();
            if (!int.TryParse(message, out int max))
            {
                await dmChannel.SendMessageAsync($"Unable to set max: \"{message}\" is not a valid number.");
            }

            config.MaxMessages = max;
            await dmChannel.SendMessageAsync($"Maximum number of messages between interjecting: \"{max}\".");
        }

        private async Task DoAdd(string message, RandomlyConfig config)
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
            config.RandomMessages.Add(newMessage);
            var dmChannel = await Context.User.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"\"{message}\" added.");
        }

        private async Task DoRemove(string message, RandomlyConfig config)
        {
            var toRemove = config.RandomMessages.SingleOrDefault(a => a.Message == message);

            if (toRemove == null)
            {
                var dmChannel = await Context.User.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync($"\"{message}\" was not found.");
            }
            else
            {
                config.RandomMessages.Remove(toRemove);
            }
        }

    }
}
