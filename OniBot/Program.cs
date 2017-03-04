using Microsoft.Extensions.DependencyInjection;
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
            AsyncPump.Run(a => program.MainAsync(args), args);
            Console.ReadKey();
        }

        private Program()
        {
            _serviceCollection = new ServiceCollection();
        }

        private readonly IServiceCollection _serviceCollection;
        private IServiceProvider _serviceProvider;

        private async Task MainAsync(string[] args)
        {
            var startup = new Startup(args);
            startup.ConfigureServices(_serviceCollection);
            _serviceProvider = _serviceCollection.BuildServiceProvider(true);

            try
            {
                using (var bot = _serviceProvider.GetService<DiscordBot>())
                {
                    await bot.Run();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}