using Devkoes.JenkinsManager.Model.Contract;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Devkoes.JenkinsManager.VSPackage.ExposedServices
{
    public class OutputWindowLogger : IOutputWindowLogger
    {
        internal const string OUTPUTWINDOW_ID = "D252353A-6121-4AC7-8B0E-316A0571ED68";
        private const string _outputWindowTitle = "Jenkins Manager";
        private IVsOutputWindowPane _outputWindow;

        public OutputWindowLogger()
        {
            try
            {
                var outputWindowGuid = new Guid(OUTPUTWINDOW_ID);

                IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                outWindow.CreatePane(ref outputWindowGuid, _outputWindowTitle, 1, 1);

                outWindow.GetPane(ref outputWindowGuid, out _outputWindow);

                _outputWindow.OutputString("Jenkins Manager loaded");
            }
            catch (Exception)
            {
                // We don't want any exception to propagate, cause that will result in vs crash
                // but there is nothing left to do, cause logging won't work.
            }
        }

        public void LogOutput(string message)
        {
            try
            {
                _outputWindow.OutputString(message);
            }
            catch { }
        }

        public void LogOutput(string format, params object[] args)
        {
            try
            {
                _outputWindow.OutputString(string.Format(format, args));
            }
            catch { }
        }

        public void LogOutput(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            try
            {
                _outputWindow.OutputString(ex.ToString());
            }
            catch { }
        }

        public void LogOutput(string message, Exception ex)
        {
            LogOutput(message);
            LogOutput(ex);
        }
    }
}
