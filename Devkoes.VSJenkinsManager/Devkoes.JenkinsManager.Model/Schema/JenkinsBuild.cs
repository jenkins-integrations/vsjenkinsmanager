using Newtonsoft.Json;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsBuild
    {
        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("Building")]
        public bool IsBuilding { get; set; }

        [JsonProperty("EstimatedDuration")]
        public long EstimatedDuration { get; set; }

        [JsonProperty("Timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("Result")]
        [JsonConverter(typeof(JenkinsBuildResultConverter))]
        public bool IsSuccessfull { get; set; }

        [JsonIgnore]
        public string StatusColor
        {
            get
            {
                return IsSuccessfull ? "ForestGreen" : "Firebrick";
            }
        }
    }
}
