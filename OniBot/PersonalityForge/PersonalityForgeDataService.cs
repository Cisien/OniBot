using System;
using System.Net;
using JamesWright.PersonalityForge.Models;
using JamesWright.PersonalityForge.Interfaces;
using System.Threading.Tasks;
using System.Net.Http;

namespace JamesWright.PersonalityForge
{
	internal class PersonalityForgeDataService : IPersonalityForgeDataService
	{
		private const string _host = "http://www.personalityforge.com/api/chat/";
		private HttpClient _client;
		private IUtils _utils;

		public PersonalityForgeDataService()
		{
            _client = new HttpClient();
			_utils = new Utils();
		}

		//constructor for dependency injection
		public PersonalityForgeDataService(IUtils utils)
		{
			_client = new HttpClient();
			_utils = utils;
		}

		public async Task<Response> SendAsync(ApiInfo apiInfo, string username, string text)
		{
			Message message = CreateMessage(text, apiInfo.BotId);
			User user = CreateUser(username);

			Payload data = CreatePayload(message, user);

			string dataJson = data.ToJson();
			string request = GetRequestUri(apiInfo, dataJson);

			try
			{
				string responseJson = await MakeRequestAsync(request);
				return responseJson.ToObject<Response>();
			}
			catch (Exception e)
			{
				throw new PersonalityForgeException(e.Message, e);
			}
		}

		private async Task<string> MakeRequestAsync(string request)
		{
			try
			{
				return await _client.GetStringAsync(request);
			}
			catch (WebException e)
			{
				throw new PersonalityForgeException(e.Message, e);
			}
			catch (Exception e)
			{
				throw new PersonalityForgeException(e.Message, e);
			}
		}

		private User CreateUser(string username)
		{
			return new User
			{
				ExternalID = username
			};
		}

		private Message CreateMessage(string text, int botId)
		{
			return new Message
			{
				Text = text,
				Timestamp = _utils.GenerateTimestamp(),
				ChatBotId = botId
			};
		}

		private Payload CreatePayload(Message message, User user)
		{
			return new Payload
			{
				Message = message,
				User = user
			};
		}

		private string GetRequestUri(ApiInfo apiInfo, string dataJson)
		{
			return $"{_host}?apiKey={apiInfo.Key}&hash={_utils.GenerateSecret(apiInfo.Secret, dataJson)}&message={WebUtility.UrlEncode(dataJson)}";
		}
	}
}
