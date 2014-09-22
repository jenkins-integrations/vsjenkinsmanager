using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Schema;
using Devkoes.JenkinsManager.UI.ViewModels;
using System;

namespace Devkoes.JenkinsManager.UI.Managers
{
    public class SolutionManager
    {
        private static Lazy<SolutionManager> _instance;
        public string _currentSolutionPath;

        public event EventHandler<SolutionChangedEventArgs> SolutionPathChanged;

        static SolutionManager()
        {
            _instance = new Lazy<SolutionManager>(() => new SolutionManager());
        }

        private SolutionManager()
        {
            // close instantiation, singleton use only
        }

        public string CurrentSolutionPath
        {
            get { return _currentSolutionPath; }
            set
            {
                if (value != _currentSolutionPath)
                {
                    _currentSolutionPath = value;
                    if (SolutionPathChanged != null)
                    {
                        SolutionPathChanged(this, new SolutionChangedEventArgs() { SolutionPath = _currentSolutionPath });
                    }
                }
            }
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
            SolutionJob sJob = SettingManager.GetJobUri(slnPath);
            await ViewModelController.JenkinsManagerViewModel.ScheduleJob(sJob.JobUrl, sJob.JenkinsServerUrl);
        }
    }
}
