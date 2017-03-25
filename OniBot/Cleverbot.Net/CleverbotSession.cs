using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cleverbot.Net
{
    public class CleverbotSession
    {

        private string ApiKey { get; }
        
        /// <summary>
        /// Creates a Cleverbot instance.
        /// </summary>
        /// <param name="apikey">Your api key obtained from https://cleverbot.com/api/</param>
        /// <param name="sendTestMessage">Send a test message to be sure you're connected</param>
        public CleverbotSession(string apikey, bool sendTestMessage = true)
        {
            if (string.IsNullOrWhiteSpace(apikey))
            {
                throw new Exception("You can't connect without a API key.");
            }

            ApiKey = apikey;

            /*
             * If you're going to work async, it might throw random errors e.g. Invocation error. 
             * Use this test message to confirm it works.
             * jeuxjeux20's note : I would suggest making test units
             */ 
            if (sendTestMessage)
            {
               var test = GetResponseAsync("test").Result;
            }
        }

        /// <summary>

        /// Send a message to cleverbot and get a response to it. Consider using <see cref="GetResponseAsync(string)"/>
        /// </summary>
        /// <param name="message">your message sent to cleverbot</param>
        /// <returns>response from the cleverbot.com api</returns>
        [Obsolete("Use GetResponseAsync instead.")]
        public CleverbotResponse GetResponse(string message)
        {
            return CleverbotResponse.CreateAsync(message, "", ApiKey).Result;
        }

        /// <summary>
        /// Send a message to cleverbot asynchronously and get a response.
        /// </summary>
        /// <param name="message">your message sent to cleverbot</param>
        /// <returns></returns>
        public async Task<CleverbotResponse> GetResponseAsync(string message)
        {
            return await CleverbotResponse.CreateAsync(message, "", ApiKey);
        }
    }
}
