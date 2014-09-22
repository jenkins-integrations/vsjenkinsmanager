
namespace Devkoes.JenkinsManager.Model.Contract
{
    interface ISolutionJobLinkInfo
    {
        object GetJobForSolution(string solutionPath);
        bool IsSolutionLinked(string solutionPath);
    }
}
