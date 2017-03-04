using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace OniBot.Interfaces
{
    public interface ICommandHandler
    {
        Task InstallAsync(IDependencyMap map);
        Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> message, SocketMessage socketMessage, ISocketMessageChannel channel);
        Task OnMessageReceivedAsync(SocketMessage arg);
    }
}