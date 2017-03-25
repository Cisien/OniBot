using Newtonsoft.Json;

namespace JamesWright.PersonalityForge.Models
{
	public class Response
	{
		[JsonProperty("message")]
		public Message Message { get; set; }

		[JsonProperty("success")]
		public int Success { get; set; }

		[JsonProperty("errorMessage")]
		public string ErrorMessage { get; set; }
	}
}
