using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsJob : ObservableObject
    {
        private bool _linkedToCurrentSolution;

        [JsonProperty("InQueue")]
        public bool IsQueued { get; set; }

        [JsonProperty("Buildable")]
        public bool IsEnabled { get; set; }

        [JsonProperty("Builds")]
        public IEnumerable<JenkinsBuild> Builds { get; set; }

        [JsonProperty("QueueItem")]
        public JenkinsQueueItem QueueItem { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonIgnore]
        public JenkinsBuild LatestBuild
        {
            get
            {
                return Builds == null ? null : Builds.FirstOrDefault();
            }
        }

        [JsonIgnore]
        public bool LinkedToCurrentSolution
        {
            get { return _linkedToCurrentSolution; }
            set
            {
                if (_linkedToCurrentSolution != value)
                {
                    _linkedToCurrentSolution = value;
                    RaisePropertyChanged(() => LinkedToCurrentSolution);
                }
            }
        }

        [JsonIgnore]
        public string StatusColor
        {
            get
            {
                if (LatestBuild != null && LatestBuild.IsBuilding)
                {
                    return "Yellow";
                }
                if (!IsEnabled || LatestBuild == null)
                {
                    return "Gray";
                }
                if (LatestBuild != null && LatestBuild.IsSuccessfull)
                {
                    return "ForestGreen";
                }
                if (LatestBuild != null && !LatestBuild.IsSuccessfull)
                {
                    return "Firebrick";
                }
                return "Transparent";
            }
        }
    }
}
