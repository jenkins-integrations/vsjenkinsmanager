
using Newtonsoft.Json;
namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsQueueItem
    {
        [JsonProperty("Why")]
        public string Why { get; set; }
    }
}
