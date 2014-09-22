using System;

namespace Devkoes.JenkinsManager.Model.Contract
{
    interface IOutputWindowLogger
    {
        void LogOutput(string message);
        void LogOutput(string format, params object[] args);
        void LogOutput(Exception ex);
    }
}
