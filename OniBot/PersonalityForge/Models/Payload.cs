using Newtonsoft.Json;

namespace JamesWright.PersonalityForge.Models
{
	internal class Payload
	{
		[JsonProperty("message")]
		internal Message Message { get; set; }

		[JsonProperty("user")]
		internal User User { get; set; }
	}
}

