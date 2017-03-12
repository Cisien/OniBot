using OniBot.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OniBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new Program();
            AsyncPump.Run(program.MainAsync, args);
        }
        
        private CancellationTokenSource cts = new CancellationTokenSource();

        private Program()
        { }

        private async Task MainAsync(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
            };

            var host = new HostingEnvironment()
                .UseDependencyMap(new ServiceProviderDependencyMap())
                .UseCommandLineOptions(args)
                .UseLoggerFactory(new LoggerFactory())
                .UseStartup<Startup>()
                .UseBot<DiscordBot>();

            try
            {
                await host.RunAsync(cts.Token);
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }
    }
}