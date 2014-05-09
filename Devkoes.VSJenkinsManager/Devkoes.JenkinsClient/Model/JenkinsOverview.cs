using System.Collections.Generic;

namespace Devkoes.JenkinsClient.Model
{
    public class Job
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }
    }

    public class JenkinsOverview
    {
        public List<Job> Jobs { get; set; }
    }
}
