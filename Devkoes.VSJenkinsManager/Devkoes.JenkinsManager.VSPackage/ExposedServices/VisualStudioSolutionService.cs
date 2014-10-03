using Devkoes.JenkinsManager.Model.Contract;
using Devkoes.JenkinsManager.Model.Schema;
using EnvDTE;
using System;

namespace Devkoes.JenkinsManager.VSPackage.ExposedServices
{
    public class VisualStudioSolutionService : IVisualStudioSolutionEvents, IVisualStudioSolutionInfo
    {
        private DTE _currentDTE;
        private SolutionEvents _solutionEvents;

        public event EventHandler<SolutionChangedEventArgs> SolutionChanged;

        public VisualStudioSolutionService()
        {
            _currentDTE = VSJenkinsManagerPackage.Instance.GetService<DTE>();

            _solutionEvents = _currentDTE.Events.SolutionEvents;
            _solutionEvents.Opened += FireSolutionChangedEvent;
            _solutionEvents.AfterClosing += FireSolutionChangedEvent;
            _solutionEvents.Renamed += RenamedSolution;
        }

        public string SolutionPath
        {
            get { return GetSolutionPath(); }
        }

        private void RenamedSolution(string OldName)
        {
            FireSolutionChangedEvent();
        }

        private void FireSolutionChangedEvent()
        {
            if (SolutionChanged != null)
            {
                SolutionChanged(this, new SolutionChangedEventArgs() { SolutionPath = GetSolutionPath() });
            }
        }

        private string GetSolutionPath()
        {
            if (_currentDTE != null && _currentDTE.Solution != null)
            {
                return _currentDTE.Solution.FullName;
            }
            return null;
        }
    }
}
