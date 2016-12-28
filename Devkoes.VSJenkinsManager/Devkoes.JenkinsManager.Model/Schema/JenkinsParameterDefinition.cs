using Newtonsoft.Json;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsParameterDefinition
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("DefaultParameterValue")]
        public JenkinsDefaultParameterValue DefaultParameterValue { get; set; }

    }
}