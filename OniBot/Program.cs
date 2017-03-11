using Discord.Commands;
using OniBot.Infrastructure;
using System;
using System.Threading.Tasks;

namespace OniBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new Program();
            AsyncPump.Run(program.MainAsync, args);
        }

        private readonly IDependencyMap _depMap;
        private bool _run = true;

        private Program()
        {
            _depMap = new ServiceProviderDependencyMap();
        }

        private async Task MainAsync(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                _run = false;
            };

            var startup = new Startup(args);
            startup.ConfigureServices(_depMap);
           
            try
            {
                using (var bot = _depMap.Get<IDiscordBot>())
                {
                    await bot.RunBotAsync();

                    while (_run)
                    {
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            Console.ReadKey();
        }
    }
}