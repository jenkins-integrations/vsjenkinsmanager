using Newtonsoft.Json;
using System.Collections.Generic;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsView
    {
        private IList<JenkinsJob> _jobs;

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        public JenkinsView()
        {
            _jobs = new List<JenkinsJob>();
        }

        [JsonProperty("Jobs")]
        public IList<JenkinsJob> Jobs
        {
            get { return _jobs; }
            set { _jobs = value ?? new List<JenkinsJob>(); }
        }
    }
}
