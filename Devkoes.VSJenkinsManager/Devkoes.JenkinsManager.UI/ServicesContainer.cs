using Devkoes.JenkinsManager.Model.Contract;
using Devkoes.JenkinsManager.UI.ExposedServices;

namespace Devkoes.JenkinsManager.UI
{
    /// <summary>
    /// Mediator between this UI assembly and the package assembly. We need services from the one package
    /// in the other and vice versa. We could use a IoT container (Unity/MEF) when we need more flexibility. At
    /// the moment these properties are set at the time the services become available (eg package is initialized).
    /// </summary>
    public static class ServicesContainer
    {
        private static ISolutionJenkinsJobLinkInfo _slnJobLinkInfo;

        public static IOutputWindowLogger OutputWindowLogger { get; set; }
        public static IVisualStudioSolutionEvents VisualStudioSolutionEvents { get; set; }
        public static IVisualStudioSolutionInfo VisualStudioSolutionInfo { get; set; }
        public static IVisualStudioWindowHandler VisualStudioWindowHandler { get; set; }

        static ServicesContainer()
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
