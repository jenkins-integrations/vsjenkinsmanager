using Devkoes.JenkinsClient;
using Devkoes.JenkinsManagerUI.ViewModels;
using System;

namespace Devkoes.JenkinsManagerUI.Managers
{
    public class SolutionManager
    {
        private static Lazy<SolutionManager> _instance;

        public string CurrentSolutionPath { get; set; }

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
            return SettingManager.ContainsSolutionPreference(slnPath);
        }

        public async void StartJenkinsBuildForSolution(string slnPath)
        {
            string jobUri = SettingManager.GetJobUri(slnPath);
            await ViewModelController.JenkinsManagerViewModel.ScheduleJob(jobUri);
        }
    }
}
