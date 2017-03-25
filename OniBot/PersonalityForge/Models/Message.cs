using Newtonsoft.Json;

namespace JamesWright.PersonalityForge.Models
{
	public class Message
	{
		[JsonProperty("message")]
		public string Text { get; set; }

		[JsonProperty("chatBotID")]
		public int ChatBotId { get; set; }

		[JsonProperty("chatBotName")]
		public string ChatBotName { get; set; }

		[JsonProperty("timestamp")]
		public int Timestamp { get; set; }
	}
}
