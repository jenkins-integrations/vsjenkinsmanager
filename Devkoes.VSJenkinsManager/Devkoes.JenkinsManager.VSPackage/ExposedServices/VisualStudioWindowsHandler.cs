using Devkoes.JenkinsManager.Model.Contract;
using Devkoes.JenkinsManager.UI;
using Devkoes.JenkinsManager.UI.Views;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Devkoes.JenkinsManager.VSPackage.ExposedServices
{
    public class VisualStudioWindowHandler : IVisualStudioWindowHandler
    {
        public void ShowToolWindow()
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = VSJenkinsManagerPackage.Instance.FindToolWindow(typeof(JenkinsToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public void ShowSettingsWindow()
        {
            try
            {
                VSJenkinsManagerPackage.Instance.ShowOptionPage(typeof(UserOptionsHost));
            }
            catch (Exception ex)
            {
                Logger.Log("Showing settings panel failed:", ex);
            }
        }

        public void ShowOutputWindow()
        {
            try
            {
                ActivateOutputWindow();

                ActivateJenkinsOutputPane();
            }
            catch (Exception ex)
            {
                Logger.Log("Showing output panel failed:", ex);
            }
        }

        private static void ActivateJenkinsOutputPane()
        {
            var outputWindowGuid = new Guid(OutputWindowLogger.OUTPUTWINDOW_ID);
            IVsOutputWindowPane outputWindow;
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            outWindow.GetPane(ref outputWindowGuid, out outputWindow);
            outputWindow.Activate();
        }

        private static void ActivateOutputWindow()
        {
            var currentDTE = VSJenkinsManagerPackage.Instance.GetService<DTE>();
            Windows windows = currentDTE.Windows;
            Window window = (Window)windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            window.Activate();
        }
    }
}
