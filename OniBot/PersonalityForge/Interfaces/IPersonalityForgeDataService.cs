using JamesWright.PersonalityForge.Models;
using System.Threading.Tasks;

namespace JamesWright.PersonalityForge.Interfaces
{
    internal interface IPersonalityForgeDataService
    {
        Task<Response> SendAsync(ApiInfo apiInfo, string username, string text);
    }
}
