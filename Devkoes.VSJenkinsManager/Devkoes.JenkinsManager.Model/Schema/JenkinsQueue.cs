using System.Collections.Generic;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsQueue
    {
        public IEnumerable<JenkinsScheduledJob> Items { get; set; }
    }
}
