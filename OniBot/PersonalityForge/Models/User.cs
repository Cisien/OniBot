using Newtonsoft.Json;

namespace JamesWright.PersonalityForge.Models
{
	internal class User
	{
		[JsonProperty("externalID")]
		public string ExternalID;
	}
}

