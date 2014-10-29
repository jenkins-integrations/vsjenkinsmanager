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
            SolutionJenkinsJobLink sJob = ApiHandlerSettingsManager.GetJobLink(slnPath);
            await ViewModelController.JenkinsManagerViewModel.BuildJob(sJob.JobUrl, sJob.JenkinsServerUrl);
        }

        public bool IsSolutionLinked(string solutionPath)
        {
            return ApiHandlerSettingsManager.ContainsSolutionPreference(solutionPath);
        }
    }
}
