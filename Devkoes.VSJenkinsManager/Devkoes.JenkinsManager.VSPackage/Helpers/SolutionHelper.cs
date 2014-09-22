using Devkoes.JenkinsManager.UI.Managers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Devkoes.JenkinsManager.VSPackage.Helpers
{
    public delegate void SolutionRenamed(Solution currentSolution, string oldName);

    public class SolutionHelper
    {
        private const string _outputWindowId = "D252353A-6121-4AC7-8B0E-316A0571ED68";
        private const string _outputWindowTitle = "Jenkins Manager";
        private IVsOutputWindowPane _outputWindow;
        private Guid _outputWindowGuid;
        private DTE _currentDTE;
        private static Lazy<SolutionHelper> _instance;

        private SolutionEvents _solutionEvents;

        static SolutionHelper()
        {
            _instance = new Lazy<SolutionHelper>(() => new SolutionHelper());
        }

        public SolutionHelper()
        {
            _outputWindowGuid = new Guid(_outputWindowId);
        }

        internal void Initialize()
        {
            _currentDTE = VSJenkinsManagerPackage.Instance.GetService<DTE>();

            _solutionEvents = _currentDTE.Events.SolutionEvents;
            _solutionEvents.Opened += OpenedSolution;
            _solutionEvents.AfterClosing += AfterClosingSolution;
            _solutionEvents.Renamed += RenamedSolution;

            SolutionManager.Instance.CurrentSolutionPath = GetSolutionPath();

            InitializeOutputWindow();
        }

        private void InitializeOutputWindow()
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            string customTitle = _outputWindowTitle;
            outWindow.CreatePane(ref _outputWindowGuid, customTitle, 1, 1);

            outWindow.GetPane(ref _outputWindowGuid, out _outputWindow);

            _outputWindow.OutputString("Jenkins Manager loaded");
        }

        public static SolutionHelper Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private void AfterClosingSolution()
        {
            SolutionManager.Instance.CurrentSolutionPath = GetSolutionPath();
        }

        private void RenamedSolution(string OldName)
        {
            SolutionManager.Instance.CurrentSolutionPath = GetSolutionPath();
        }

        private void OpenedSolution()
        {
            SolutionManager.Instance.CurrentSolutionPath = GetSolutionPath();
        }

        public void WriteOutput(string text)
        {
            _outputWindow.OutputString(text);
        }

        public Solution GetSolution()
        {
            return _currentDTE.Solution;
        }

        public void ShowJenkinsToolWindow()
        {
            VSJenkinsManagerPackage.Instance.ShowToolWindow(this, new EventArgs());
        }

        public string GetSolutionPath()
        {
            if (_currentDTE != null && _currentDTE.Solution != null)
            {
                return _currentDTE.Solution.FullName;
            }
            return null;
        }
    }
}
