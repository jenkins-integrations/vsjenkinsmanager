
namespace Devkoes.JenkinsManager.Model.Contract
{
    public interface ISolutionJenkinsJobLinkInfo
    {
        void StartJenkinsBuildForSolution(string solutionPath);
        bool IsSolutionLinked(string solutionPath);
    }
}
