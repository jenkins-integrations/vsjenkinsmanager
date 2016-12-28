using Newtonsoft.Json;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsDefaultParameterValue
    {
        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
