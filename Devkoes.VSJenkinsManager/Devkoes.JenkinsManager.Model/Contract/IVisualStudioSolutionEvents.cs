using Devkoes.JenkinsManager.Model.Schema;
using System;

namespace Devkoes.JenkinsManager.Model.Contract
{
    interface IVisualStudioSolutionEvents
    {
        event EventHandler<SolutionChangedEventArgs> SolutionChanged;
    }
}
