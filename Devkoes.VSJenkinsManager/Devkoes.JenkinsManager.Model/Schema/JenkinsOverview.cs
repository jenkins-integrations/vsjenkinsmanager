using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsOverview
    {
        private IEnumerable<JenkinsView> _views = Enumerable.Empty<JenkinsView>();

        [JsonProperty("Views")]
        public IEnumerable<JenkinsView> Views
        {
            get { return _views; }
            set { _views = value ?? Enumerable.Empty<JenkinsView>(); }
        }
    }
}
