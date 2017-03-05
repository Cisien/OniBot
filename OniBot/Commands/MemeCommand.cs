using Discord.Commands;
using OniBot.Interfaces;
using System;
using System.Threading.Tasks;
using Discord;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;

namespace OniBot.Commands
{
    public class MemeCommand : ModuleBase, IBotCommand
    {
        private static Random _random = new Random();
        private static readonly Regex galleryRegex = new Regex(@"(/gallery/\w+)", RegexOptions.Compiled);
        private static readonly Regex imageRegex = new Regex(@"//i.imgur.com(?<img>/\w+).(?<ext>png|jpg|gif)"".+alt=""(?<desc>.+)"" \w", RegexOptions.Compiled);
        private static HttpClient client = new HttpClient();

        [Command("randommeme")]
        [Summary("Get dank, son!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task Meme([Remainder] string search)
        {
            var img = await FindImage($"meme+{search}");
            if (img == null)
            {
                await Context.Channel.SendMessageAsync("Someone lashed out against our meme!");
                return;
            }

            try
            {
                await Context.Channel.SendFileAsync(img.Filename, img.Description);
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

                var data = await client.GetAsync(searchLink);

                var input = await data.Content.ReadAsStringAsync();

                var galleryMatches = galleryRegex.Matches(input);
                if (galleryMatches.Count == 0)
                {
                    return null;
                }

                int index = _random.Next(0, galleryMatches.Count);
                
                string galleryItemLink = $"http://imgur.com{galleryMatches[index].Value}";
                Console.WriteLine($"calling {galleryItemLink}");

                var galleryItemResult = await client.GetStringAsync(galleryItemLink);
                var galleryPageMatches = imageRegex.Matches(galleryItemResult);

                if (galleryPageMatches.Count == 0)
                {
                    return null;
                }

                index = _random.Next(0, galleryPageMatches.Count);
                var match = galleryPageMatches[index];

                var imageLink = $"http://i.imgur.com{match.Groups["img"].Value}.{match.Groups["ext"].Value}";
                Console.WriteLine($"calling {imageLink}");

                var image = await client.GetByteArrayAsync(imageLink);
                var temp = $"{Guid.NewGuid()}.{match.Groups["ext"].Value}";
                
                File.WriteAllBytes(temp, image);

                return new ImageResult { Filename = temp, Description = match.Groups["desc"]?.Value };
            }
            catch (Exception e)
            {
                DiscordBot.Log(nameof(FindImage), LogSeverity.Error, e.ToString());
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
