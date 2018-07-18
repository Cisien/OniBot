using System.Threading.Tasks;

namespace OniBot
{
    public interface IBehaviorService
    {
        Task InstallAsync();
        Task RunAsync();
        Task StopAsync();
    }
}