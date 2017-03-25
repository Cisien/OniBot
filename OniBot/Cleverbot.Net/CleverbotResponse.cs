using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Cleverbot.Net
{
    public class CleverbotResponse
    {
        #region internals
        /*
         Json.NET automatically detects names using underscores and camelCase, so no need for unnecessary attributes ;)

         Oh, I didn't know this! 
         
         It couldn't find output or conversationid :(
        */

         [JsonProperty("interaction_count")]
          internal string interactionCount;
  
         [JsonProperty("input")]
          internal string inputMessage;
  
         [JsonProperty("predicted_input")]
         // ("predicted_input")]
          internal string predictedInputMessage;
       
        // ("accuracy")]
        internal string accuracy;

        // ("output_label")]
        internal string outputLabel;

        [JsonProperty("output")]
        internal string output;

        [JsonProperty("conversation_id")]
        internal string conversationId;
        
        // ("errorline")]
        internal string errorLine;

        // ("database_version")]
        internal string databaseVersion;

        // ("software_version")]
        internal string softwareVersion;

        // ("time_taken")]
        internal string timeTaken;

        // ("random_number")]
        internal string randomNumber;

        [JsonProperty("time_second")]
        internal string timeSeconds;

        [JsonProperty("time_minute")]
        internal string timeMinutes;

        [JsonProperty("time_hour")]
        internal string timeHours;

        // ("time_day_of_week")]
        internal string timeDayOfWeek;

        [JsonProperty("time_day")]
        internal string timeDays;

       [JsonProperty("time_month")]
        internal string timeMonths;

        [JsonProperty("time_year")]
        internal string timeYears;

        // ("reaction")]
        internal string reaction;

        // ("reaction_tone")]
        internal string reactionTone;

        // ("emotion")]
        internal string emotion;

        // ("emotion_tone")]
        internal string emotionTone;

        // ("clever_accuracy")]
        internal string cleverAccuracy;

        // ("clever_output")]
        internal string cleverOutput;

        // ("clever_match")]
        internal string cleverMatch;

        // ("time_elapsed")]
        internal string timeElapsed;

        // ("filtered_input")]
        internal string filteredInput;

        // ("reaction_degree")]
        internal string reactionDegree;

        // ("emotion_degree")]
        internal string emotionDegree;

        // ("reaction_values")]
        internal string reactionValues;

        // ("emotion_values")]
        internal string emotionValues;

        // ("callback")]
        internal string callback;

        // TODO: convince Rollo to make these a array/list in json
        // It failed
        // ("interaction_1")]
        internal string interaction1;

        // ("interaction_2")]
        internal string interaction2;

        // ("interaction_3")]
        internal string interaction3;

        // ("interaction_4")]
        internal string interaction4;

        // ("interaction_5")]
        internal string interaction5;

        // ("interaction_6")]
        internal string interaction6;

        // ("interaction_7")]
        internal string interaction7;

        // ("interaction_8")]
        internal string interaction8;

        // ("interaction_9")]
        internal string interaction9;

        // ("interaction_10")]
        internal string interaction10;

        // ("interaction_11")]
        internal string interaction11;

        // ("interaction_12")]
        internal string interaction12;

        // ("interaction_13")]
        internal string interaction13;

        // ("interaction_14")]
        internal string interaction14;

        // ("interaction_15")]
        internal string interaction15;

        // ("interaction_16")]
        internal string interaction16;

        // ("interaction_17")]
        internal string interaction17;

        // ("interaction_18")]
        internal string interaction18;

        // ("interaction_19")]
        internal string interaction19;

        // ("interaction_20")]
        internal string interaction20;

        // ("interaction_21")]
        internal string interaction21;

        // ("interaction_22")]
        internal string interaction22;

        // ("interaction_23")]
        internal string interaction23;

        // ("interaction_24")]
        internal string interaction24;

        // ("interaction_25")]
        internal string interaction25;

        // ("interaction_26")]
        internal string interaction26;

        // ("interaction_27")]
        internal string interaction27;

        // ("interaction_28")]
        internal string interaction28;

        // ("interaction_29")]
        internal string interaction29;

        // ("interaction_30")]
        internal string interaction30;

        // ("interaction_31")]
        internal string interaction31;

        // ("interaction_32")]
        internal string interaction32;

        // ("interaction_33")]
        internal string interaction33;

        // ("interaction_34")]
        internal string interaction34;

        // ("interaction_35")]
        internal string interaction35;

        // ("interaction_36")]
        internal string interaction36;

        // ("interaction_37")]
        internal string interaction37;

        // ("interaction_38")]
        internal string interaction38;

        // ("interaction_39")]
        internal string interaction39;

        // ("interaction_40")]
        internal string interaction40;

        // ("interaction_41")]
        internal string interaction41;

        // ("interaction_42")]
        internal string interaction42;

        // ("interaction_43")]
        internal string interaction43;

        // ("interaction_44")]
        internal string interaction44;

        // ("interaction_45")]
        internal string interaction45;

        // ("interaction_46")]
        internal string interaction46;

        // ("interaction_47")]
        internal string interaction47;

        // ("interaction_48")]
        internal string interaction48;

        // ("interaction_49")]
        internal string interaction49;

        // ("interaction_50")]
        internal string interaction50;

        // ("interaction_1_other")]

        internal string interaction1other;

        internal List<string> interactions = new List<string>();

        #endregion

        /// <summary>
        /// Id to keep track of the conversation
        /// </summary>
        public string ConversationId => conversationId;

        /// <summary>
        /// Cleverbot's response message
        /// </summary>
        public string Response => output;

        /// <summary>
        /// The user's latest message
        /// </summary>
        public string Input => inputMessage;

        private string apiKey;

        internal static async Task<CleverbotResponse> CreateAsync(string message, string conversationId, string apiKey)
        {
            HttpClient c = new HttpClient();

            string conversationLine = (string.IsNullOrWhiteSpace(conversationId) ? "" : $"&cs={conversationId}");
            
            string result = await c.GetStringAsync($"https://www.cleverbot.com/getreply?key={ apiKey }&wrapper=cleverbot.net&input={ message }{ conversationLine }");
            CleverbotResponse response = JsonConvert.DeserializeObject<CleverbotResponse>(result);
            if (response == null) return null;
            response.apiKey = apiKey;
            response.CreateInteractionsList();

            return response;
        }

        /*internal static async Task<CleverbotResponse> CreateAsync(string message, string conversationId, string apiKey)
            => await Create(message, conversationId, apiKey);*/      

        internal void CreateInteractionsList()
        {
           
            foreach (var item in GetType().GetTypeInfo().GetFields())
            {
                if (item.Name.StartsWith("interaction"))
                {
                    if (string.IsNullOrWhiteSpace((string) item.GetValue(this)))
                    {
                        interactions.Add(item.GetValue(this) as string);
                    }
                }
            }
        }

        public CleverbotResponse Respond(string text)
        {
            return CreateAsync(text, ConversationId, apiKey).Result;
        }

        public async Task<CleverbotResponse> RespondAsync(string text)
        {
            return await CreateAsync(text, conversationId, apiKey);
        }
       

    }
}
