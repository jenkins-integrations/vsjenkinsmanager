using System.Collections.Generic;

namespace Devkoes.JenkinsClient.Model
{
    public class JenkinsQueue
    {
        public IEnumerable<ScheduledJob> Items { get; set; }
    }

    public class ScheduledJob
    {
        public Job Task { get; set; }
    }
}
