using System.Collections.Generic;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsView
    {
        private IList<JenkinsJob> _jobs;

        public string Name { get; set; }
        public string Url { get; set; }

        public JenkinsView()
        {
            _jobs = new List<JenkinsJob>();
        }

        public IList<JenkinsJob> Jobs
        {
            get { return _jobs; }
            set { _jobs = value ?? new List<JenkinsJob>(); }
        }
    }
}
