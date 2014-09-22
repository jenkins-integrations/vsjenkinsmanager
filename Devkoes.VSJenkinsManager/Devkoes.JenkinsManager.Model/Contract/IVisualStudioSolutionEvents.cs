using Devkoes.JenkinsManager.Model.Schema;
using System;

namespace Devkoes.JenkinsManager.Model.Contract
{
    public interface IVisualStudioSolutionEvents
    {
        event EventHandler<SolutionChangedEventArgs> SolutionChanged;
    }
}
