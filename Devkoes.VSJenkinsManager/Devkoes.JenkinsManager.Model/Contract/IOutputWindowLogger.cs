using System;

namespace Devkoes.JenkinsManager.Model.Contract
{
    public interface IOutputWindowLogger
    {
        void LogOutput(string message);
        void LogOutput(string format, params object[] args);
        void LogOutput(Exception ex);
        void LogOutput(string message, Exception ex);
    }
}
