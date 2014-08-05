using System;

namespace Devkoes.JenkinsManagerUI.Managers
{
    public class SolutionManager
    {
        private static Lazy<SolutionManager> _instance;

        static SolutionManager()
        {
            _instance = new Lazy<SolutionManager>(() => new SolutionManager());
        }

        private SolutionManager()
        {
            // close instantiation, singleton use only
        }

        public static SolutionManager Instance
        {
            get { return _instance.Value; }
        }

        public bool SolutionIsConnected(string slnPath)
        {
            return false;
        }

        public void StartJenkinsBuildForSolution(string slnPath)
        {

        }
    }
}
