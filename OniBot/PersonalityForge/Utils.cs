using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using JamesWright.PersonalityForge.Interfaces;

namespace JamesWright.PersonalityForge
{
	class Utils : IUtils
	{
		public string GenerateSecret(string secret, string data)
		{
			HMACSHA256 sha = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
			byte[] messageBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
			
			StringBuilder builder = new StringBuilder();
			
			foreach (byte chunk in messageBytes)
			{
				builder.Append(chunk.ToString("x2"));
			}
			
			return builder.ToString();
		}

		public int GenerateTimestamp()
		{
			long ticks = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
			ticks /= 10000000;
			return (int)ticks;
		}

		public string FilterJson(byte[] response)
		{
			try
			{
				string resString = Encoding.UTF8.GetString(response);
				Match match = Regex.Match(resString, "{\"success\".*}");
				return match.ToString();
			}
			catch (Exception e)
			{
				throw new PersonalityForgeException(e.Message, e);
			}
		}
	}
}
