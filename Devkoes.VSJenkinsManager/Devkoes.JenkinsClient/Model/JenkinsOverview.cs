using System.Collections.Generic;

namespace Devkoes.JenkinsClient.Model
{
    public class Job
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }
        public bool Building { get; set; }
    }

    public class JenkinsOverview
    {
        public IEnumerable<Job> Jobs { get; set; }
    }
}
