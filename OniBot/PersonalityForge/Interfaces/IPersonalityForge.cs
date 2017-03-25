using System.Threading.Tasks;
using JamesWright.PersonalityForge.Models;

namespace JamesWright.PersonalityForge.Interfaces
{
    public interface IPersonalityForge
    {
        Task<Response> SendAsync(string username, string message);
    }
}
