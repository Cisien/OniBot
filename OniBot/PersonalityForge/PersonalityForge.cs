using System;
using JamesWright.PersonalityForge.Models;
using JamesWright.PersonalityForge.Interfaces;
using System.Threading.Tasks;

namespace JamesWright.PersonalityForge
{
	public class PersonalityForge : IPersonalityForge
	{
		private ApiInfo _apiInfo;
		private IPersonalityForgeDataService _dataService;

		public PersonalityForge(string secret, string key, int botId)
		{
			_apiInfo = new ApiInfo
			{
				Secret = secret,
				Key = key,
				BotId = botId
			};

			_dataService = new PersonalityForgeDataService(new Utils());
		}

		//constructor for injecting dependencies
		internal PersonalityForge(IPersonalityForgeDataService dataService)
		{
			_dataService = dataService;
		}
        
		public async Task<Response> SendAsync(string username, string message)
		{
			Response response = await _dataService.SendAsync(_apiInfo, username, message);
			return response;
		}
	}
}

