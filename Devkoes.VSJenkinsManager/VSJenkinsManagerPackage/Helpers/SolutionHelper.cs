using EnvDTE;
using System;
using System.IO;

namespace Devkoes.VSJenkinsManagerPackage.Helpers
{
    public delegate void SolutionRenamed(Solution currentSolution, string oldName);

    public class SolutionHelper
    {
        private static DTE _currentDTE;

        public static event Action<Solution> SolutionLoaded;
        public static event SolutionRenamed SolutionRenamed;
        public static event Action<Solution> SolutionClosing;

        internal static void InitializeEvents()
        {
            _currentDTE = VSJenkinsManagerPackagePackage.Instance.GetService<DTE>();

            var events = _currentDTE.Events.SolutionEvents;
            events.Opened += OpenedSolution;
            events.BeforeClosing += BeforeClosingSolution;
            events.Renamed += RenamedSolution;
        }

        private static void BeforeClosingSolution()
        {
            if (SolutionClosing != null)
            {
                SolutionClosing(_currentDTE.Solution);
            }
        }

        private static void RenamedSolution(string OldName)
        {
            if (SolutionRenamed != null)
            {
                SolutionRenamed(_currentDTE.Solution, OldName);
            }
        }

        private static void OpenedSolution()
        {
            if (SolutionLoaded != null)
            {
                SolutionLoaded(_currentDTE.Solution);
            }
        }

        public static Solution GetSolution()
        {
            return _currentDTE.Solution;
        }

        public void ShowJenkinsToolWindow()
        {
            VSJenkinsManagerPackagePackage.Instance.ShowToolWindow(this, new EventArgs());
        }

        public static string GetSolutionName()
        {
            return Path.GetFileNameWithoutExtension(_currentDTE.FullName);
        }
    }
}
