using Newtonsoft.Json;
using System;

namespace Devkoes.JenkinsManager.Model.Schema
{
    internal class JenkinsBuildResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string result = reader.Value as string;
            if (string.Equals(result, "success", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
