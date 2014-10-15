
namespace Devkoes.JenkinsManager.Model.Schema
{
    public class SolutionJenkinsJobLink
    {
        public string SolutionPath { get; set; }
        public string JenkinsServerUrl { get; set; }
        public string JenkinsViewName { get; set; }
        public string JobUrl { get; set; }
    }
}
