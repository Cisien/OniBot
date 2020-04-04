using System.IO;
using System.Threading.Tasks;

namespace OniBot
{
    public interface IVoiceService
    {
        Task<byte[]> ToVoice(string message);
    }
}