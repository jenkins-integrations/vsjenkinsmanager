using Devkoes.JenkinsManager.Model.Contract;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Devkoes.JenkinsManager.VSPackage.ExposedServices
{
    public class OutputWindowLogger : IOutputWindowLogger
    {
        private const string _outputWindowId = "D252353A-6121-4AC7-8B0E-316A0571ED68";
        private const string _outputWindowTitle = "Jenkins Manager";
        private IVsOutputWindowPane _outputWindow;

        public OutputWindowLogger()
        {
            var outputWindowGuid = new Guid(_outputWindowId);

            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            string customTitle = _outputWindowTitle;
            outWindow.CreatePane(ref outputWindowGuid, customTitle, 1, 1);

            outWindow.GetPane(ref outputWindowGuid, out _outputWindow);

            _outputWindow.OutputString("Jenkins Manager loaded");
        }

        public void LogOutput(string message)
        {
            _outputWindow.OutputString(message);
        }

        public void LogOutput(string format, params object[] args)
        {
            _outputWindow.OutputString(string.Format(format, args));
        }

        public void LogOutput(Exception ex)
        {
            if (ex != null && !string.IsNullOrWhiteSpace(ex.Message))
            {
                _outputWindow.OutputString(ex.Message);
            }
        }
    }
}
