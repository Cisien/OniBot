using Discord.Commands;
using OniBot.Interfaces;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Extensions.Logging;
using OniBot.Infrastructure;

namespace OniBot.Commands
{
    [ConfigurationPrecondition]
    public class MemeCommand : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private static Random _random = new Random();
        private static readonly Regex galleryRegex = new Regex(@"(/gallery/\w+)", RegexOptions.Compiled);
        private static readonly Regex imageRegex = new Regex(@"//i.imgur.com(?<img>/\w+).(?<ext>png|jpg|gif)"".+alt=""(?<desc>.+)"" \w", RegexOptions.Compiled);
        private static HttpClient client = new HttpClient();
        private ILogger _logger;

        public MemeCommand(ILogger logger) {
            _logger = logger;
        }

        [Command("randommeme")]
        [Summary("Searches Imgur and provides a random image from the results.")]
        public async Task Meme(
            [Summary("The search term to submit to Imgur.")][Remainder] string search)
        {
            var img = await FindImage($"meme+{search}").ConfigureAwait(false);
            if (img == null)
            {
                await Context.Channel.SendMessageAsync("Someone lashed out against our meme!").ConfigureAwait(false);
                return;
            }

            try
            {
                await Context.Channel.SendFileAsync(img.Filename, img.Description).ConfigureAwait(false);
            }
            finally
            {
                File.Delete(img.Filename);
            }
        }

        private async Task<ImageResult> FindImage(string question)
        {
            try
            {
                var searchLink = $"http://imgur.com/search/score?q={question.Replace(" ", "+")}";
                Console.WriteLine($"calling {searchLink}");

                var data = await client.GetAsync(searchLink).ConfigureAwait(false);

                var input = await data.Content.ReadAsStringAsync().ConfigureAwait(false);

                var galleryMatches = galleryRegex.Matches(input);
                if (galleryMatches.Count == 0)
                {
                    return null;
                }

                int index = _random.Next(0, galleryMatches.Count);
                
                string galleryItemLink = $"http://imgur.com{galleryMatches[index].Value}";
                Console.WriteLine($"calling {galleryItemLink}");

                var galleryItemResult = await client.GetStringAsync(galleryItemLink).ConfigureAwait(false);
                var galleryPageMatches = imageRegex.Matches(galleryItemResult);

                if (galleryPageMatches.Count == 0)
                {
                    return null;
                }

                index = _random.Next(0, galleryPageMatches.Count);
                var match = galleryPageMatches[index];

                var imageLink = $"http://i.imgur.com{match.Groups["img"].Value}.{match.Groups["ext"].Value}";
                Console.WriteLine($"calling {imageLink}");

                var image = await client.GetByteArrayAsync(imageLink).ConfigureAwait(false);
                var temp = $"{Guid.NewGuid()}.{match.Groups["ext"].Value}";
                
                File.WriteAllBytes(temp, image);

                return new ImageResult { Filename = temp, Description = match.Groups["desc"]?.Value };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return null;
            }
        }

        private class ImageResult
        {
            public string Filename { get; set; }
            public string Description { get; set; }
        }
    }
}
