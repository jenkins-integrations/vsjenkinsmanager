using Devkoes.JenkinsManagerUI.Managers;
using EnvDTE;
using System;

namespace Devkoes.VSJenkinsManagerPackage.Helpers
{
    public delegate void SolutionRenamed(Solution currentSolution, string oldName);

    public class SolutionHelper
    {
        private DTE _currentDTE;
        private static Lazy<SolutionHelper> _instance;

        private SolutionEvents _solutionEvents;

        static SolutionHelper()
        {
            _instance = new Lazy<SolutionHelper>(() => new SolutionHelper());
        }

        internal void Initialize()
        {
            _currentDTE = VSJenkinsManagerPackagePackage.Instance.GetService<DTE>();

            _solutionEvents = _currentDTE.Events.SolutionEvents;
            _solutionEvents.Opened += OpenedSolution;
            _solutionEvents.AfterClosing += AfterClosingSolution;
            _solutionEvents.Renamed += RenamedSolution;

            SolutionManager.Instance.CurrentSolutionPath = GetSolutionPath();
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

        public Solution GetSolution()
        {
            return _currentDTE.Solution;
        }

        public void ShowJenkinsToolWindow()
        {
            VSJenkinsManagerPackagePackage.Instance.ShowToolWindow(this, new EventArgs());
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
