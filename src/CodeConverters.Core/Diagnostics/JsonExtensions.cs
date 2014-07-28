using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodeConverters.Core.Diagnostics
{
    public static class JsonExtensions
    {
        public static string ScrubJson(this string jsonText)
        {
            var json = JObject.Parse(jsonText);
            json.ScrubValues(LoggingConfig.DefaultScrubParams);
            return json.ToString(Formatting.None);
        }
        
        public static T ScrubValues<T>(this T obj, string[] scrubParams) where T : JToken
        {
            var jprop = obj as JProperty;

            if (jprop != null && jprop.Value is JValue)
            {
                if (scrubParams.Contains(jprop.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    jprop.Value = "**********";
                }
            }
            if (obj.HasValues)
            {
                foreach (var element in obj)
                {
                    ScrubValues(element, scrubParams);
                }
            }
            return obj;
        }
    }
}