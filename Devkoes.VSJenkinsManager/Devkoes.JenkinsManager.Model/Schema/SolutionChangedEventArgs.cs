using System;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class SolutionChangedEventArgs : EventArgs
    {
        public string SolutionPath { get; set; }
    }
}
