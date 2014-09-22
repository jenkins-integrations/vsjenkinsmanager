using Devkoes.JenkinsManager.Model.Contract;
using Devkoes.JenkinsManager.UI.ExposedServices;

namespace Devkoes.JenkinsManager.UI
{
    public static class DependencyContainer
    {
        private static ISolutionJenkinsJobLinkInfo _slnJobLinkInfo;

        public static IOutputWindowLogger OutputWindowLogger { get; set; }
        public static IVisualStudioSolutionEvents VisualStudioSolutionEvents { get; set; }
        public static IVisualStudioSolutionInfo VisualStudioSolutionInfo { get; set; }

        static DependencyContainer()
        {
            _slnJobLinkInfo = new SolutionJenkinsJobLinkInfo();
        }

        public static ISolutionJenkinsJobLinkInfo SolutionJenkinsJobLinkInfo
        {
            get { return _slnJobLinkInfo; }
            set { _slnJobLinkInfo = value; }
        }
    }
}
