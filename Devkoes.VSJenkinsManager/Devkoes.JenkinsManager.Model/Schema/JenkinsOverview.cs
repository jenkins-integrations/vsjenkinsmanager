using System.Collections.Generic;
using System.Linq;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsOverview
    {
        private IEnumerable<JenkinsJob> _jobs = Enumerable.Empty<JenkinsJob>();
        private IEnumerable<JenkinsView> _views = Enumerable.Empty<JenkinsView>();

        public IEnumerable<JenkinsJob> Jobs
        {
            get { return _jobs; }
            set { _jobs = value ?? Enumerable.Empty<JenkinsJob>(); }
        }

        public IEnumerable<JenkinsView> Views
        {
            get { return _views; }
            set { _views = value ?? Enumerable.Empty<JenkinsView>(); }
        }
    }
}
