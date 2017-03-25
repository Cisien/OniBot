using System;
using JamesWright.PersonalityForge.Interfaces;
using Newtonsoft.Json;

namespace JamesWright.PersonalityForge
{
    public static class JsonHelper
    { 
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T ToObject<T>(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                throw new PersonalityForgeException(e.Message, e);
            }
        }
    }
}
