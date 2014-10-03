using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Contract;
using Devkoes.JenkinsManager.Model.Schema;
using Devkoes.JenkinsManager.UI.ViewModels;

namespace Devkoes.JenkinsManager.UI.ExposedServices
{
    public class SolutionJenkinsJobLinkInfo : ISolutionJenkinsJobLinkInfo
    {
        public async void StartJenkinsBuildForSolution(string slnPath)
        {
            SolutionJenkinsJobLink sJob = SettingManager.GetJobUri(slnPath);
            await ViewModelController.JenkinsManagerViewModel.ScheduleJob(sJob.JobUrl, sJob.JenkinsServerUrl);
        }

        public bool IsSolutionLinked(string solutionPath)
        {
            return SettingManager.ContainsSolutionPreference(solutionPath);
        }
    }
}
