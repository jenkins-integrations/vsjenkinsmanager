
using Newtonsoft.Json;
namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsQueueItem
    {
        [JsonProperty("why")]
        public string Why { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }
    }
}
