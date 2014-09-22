using System.Collections.Generic;

namespace Devkoes.JenkinsManager.APIHandler.Model
{
    public class JenkinsQueue
    {
        public IEnumerable<ScheduledJob> Items { get; set; }
    }

    public class ScheduledJob
    {
        public Job Task { get; set; }
        public string Why { get; set; }
    }
}
