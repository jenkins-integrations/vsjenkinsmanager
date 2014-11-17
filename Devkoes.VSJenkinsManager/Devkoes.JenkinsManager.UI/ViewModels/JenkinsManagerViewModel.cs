using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Schema;
using Devkoes.JenkinsManager.UI.Comparers;
using Devkoes.JenkinsManager.UI.Helpers;
using Devkoes.JenkinsManager.UI.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Devkoes.JenkinsManager.UI.ViewModels
{
    public class JenkinsManagerViewModel : ViewModelBase
    {
        private int _refreshInterval = 5000;
        private JenkinsServer _selectedJenkinsServer;
        private JenkinsJob _selectedJob;
        private Timer _refreshTimer;
        private bool _loadingJobsBusy;
        private object _loadingJobsBusyLock;
        private bool _loadingFailed;
        private JenkinsView _selectedView;
        private IEqualityComparer<JenkinsJob> _jobComparer;
        private bool _jenkinsServersEnabled;
        private bool _forceRefresh;
        private ObservableCollection<JenkinsJob> _jobs;
        private IEnumerable<JenkinsView> _jenkinsViews;

        public RelayCommand ShowSettings { get; private set; }
        public RelayCommand Reload { get; private set; }

        public RelayCommand<JenkinsJob> BuildJobCommand { get; private set; }
        public RelayCommand<JenkinsJob> ScheduleBuildCommand { get; private set; }
        public RelayCommand<JenkinsJob> CancelBuildCommand { get; private set; }
        public RelayCommand<JenkinsJob> DequeueJobCommand { get; private set; }

        public RelayCommand<JenkinsJob> ShowJobsWebsite { get; private set; }
        public RelayCommand<JenkinsJob> LinkJobToCurrentSolution { get; private set; }
        public RelayCommand<JenkinsJob> ShowLatestLog { get; private set; }
        public RelayCommand ShowOutputWindow { get; private set; }

        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }

        public JenkinsManagerViewModel()
        {
            InitializeInstanceMembers();

            InitializeCommands();

            ServicesContainer.VisualStudioSolutionEvents.SolutionChanged += SolutionPathChanged;

            JenkinsServers = ApiHandlerSettingsManager.GetServers();
            SelectedJenkinsServer = JenkinsServers.FirstOrDefault();

            _refreshTimer.Start();
        }

        private void InitializeInstanceMembers()
        {
            _jobs = new ObservableCollection<JenkinsJob>();
            _jenkinsServersEnabled = true;
            _jobComparer = new JobComparer();
            _loadingJobsBusyLock = new object();

            _refreshTimer = new Timer(_refreshInterval);
            _refreshTimer.Elapsed += RefreshJobsTimerCallback;
            _refreshTimer.AutoReset = false;
        }

        private void InitializeCommands()
        {
            Reload = new RelayCommand(HandleReload);
            ShowSettings = new RelayCommand(HandleShowSettings);

            BuildJobCommand = new RelayCommand<JenkinsJob>(BuildJob, CanDoJobAction);
            ScheduleBuildCommand = new RelayCommand<JenkinsJob>(ScheduleJob, CanDoJobAction);
            CancelBuildCommand = new RelayCommand<JenkinsJob>(CancelBuild, CanDoJobAction);
            DequeueJobCommand = new RelayCommand<JenkinsJob>(DequeueBuild, CanDoJobAction);

            ShowJobsWebsite = new RelayCommand<JenkinsJob>(ShowWebsite, CanDoJobAction);
            ShowLatestLog = new RelayCommand<JenkinsJob>(HandleShowLatestLog, CanDoJobAction);
            LinkJobToCurrentSolution = new RelayCommand<JenkinsJob>(LinkJobToSolution, CanDoJobAction);
            ShowOutputWindow = new RelayCommand(HandleShowOutputWindow);
        }

        private async void HandleShowLatestLog(JenkinsJob job)
        {
            if (job.LatestBuild == null)
            {
                Logger.LogInfo("No build available to show log from.");
                return;
            }

            try
            {
                string fileName = await JenkinsJobManager.GetLatestLog(job.Url, SelectedJenkinsServer);

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    ServicesContainer.VisualStudioFileManager.OpenFile(fileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void HandleShowOutputWindow()
        {
            ServicesContainer.VisualStudioWindowHandler.ShowOutputWindow();
        }

        private void HandleShowSettings()
        {
            ServicesContainer.VisualStudioWindowHandler.ShowSettingsWindow();
        }

        public ObservableCollection<JenkinsJob> Jobs
        {
            get { return _jobs; }
        }

        public bool JenkinsServersEnabled
        {
            get { return _jenkinsServersEnabled; }
            set
            {
                if (_jenkinsServersEnabled != value)
                {
                    _jenkinsServersEnabled = value;
                    RaisePropertyChanged(() => JenkinsServersEnabled);
                }
            }
        }

        public JenkinsView SelectedView
        {
            get { return _selectedView; }
            set
            {
                if (value != _selectedView)
                {
                    _selectedView = value;
                    RaisePropertyChanged(() => SelectedView);

                    SavePreferredView();
                    JenkinsServersEnabled = false;
                    ForceReload(true);
                }
            }
        }

        private void SavePreferredView()
        {
            string slnPath = ServicesContainer.VisualStudioSolutionInfo.SolutionPath;
            if (string.IsNullOrWhiteSpace(slnPath) || SelectedView == null)
            {
                return;
            }

            bool isSaved = ApiHandlerSettingsManager.ContainsSolutionPreference(slnPath);
            if (!isSaved)
            {
                return;
            }

            var pref = ApiHandlerSettingsManager.GetJobLink(slnPath);
            if (!string.Equals(SelectedJenkinsServer.Url, pref.JenkinsServerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            ApiHandlerSettingsManager.UpdatePreferredView(slnPath, SelectedView.Name);
        }

        private void HandleReload()
        {
            ForceReload(true);
        }

        private async void RefreshJobsTimerCallback(object sender, ElapsedEventArgs e)
        {
            await LoadJenkinsJobs();
        }

        private void SolutionPathChanged(object sender, SolutionChangedEventArgs e)
        {
            bool preferredViewSelected = TrySelectPreferredView(e);

            if (!preferredViewSelected)
            {
                // If correct view was already selected, or view doesn't exists, manually update the link icon
                UpdateJobLinkedStatus(e.SolutionPath);
            }
        }

        private bool TrySelectPreferredView(SolutionChangedEventArgs e)
        {
            if (!ApiHandlerSettingsManager.ContainsSolutionPreference(e.SolutionPath))
            {
                return false;
            }

            var jobLink = ApiHandlerSettingsManager.GetJobLink(e.SolutionPath);
            var jobLinkServer = JenkinsServers.FirstOrDefault((s) => string.Equals(s.Url, jobLink.JenkinsServerUrl, StringComparison.InvariantCultureIgnoreCase));

            if (jobLinkServer == null)
            {
                // server has been removed
                return false;
            }

            bool preferredViewSelected = false;
            if (SelectedJenkinsServer == jobLinkServer)
            {
                // Correct server already selected, just fix the view
                var preferredView = _jenkinsViews.FirstOrDefault((j) => string.Equals(j.Name, jobLink.JenkinsViewName));
                if (preferredView != null)
                {
                    SelectedView = preferredView;
                    preferredViewSelected = true;
                }
            }
            else
            {
                SelectNewJenkinsServer(jobLinkServer, jobLink.JenkinsViewName);
                preferredViewSelected = true;
            }

            return preferredViewSelected;
        }

        private void UpdateJobLinkedStatus(string slnPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(slnPath))
                {
                    slnPath = ServicesContainer.VisualStudioSolutionInfo.SolutionPath;
                }

                SolutionJenkinsJobLink sJob = ApiHandlerSettingsManager.GetJobLink(slnPath);

                UIHelper.InvokeUI(() =>
                {
                    foreach (var job in Jobs)
                    {
                        job.LinkedToCurrentSolution =
                            sJob != null &&
                            string.Equals(job.Url, sJob.JobUrl, System.StringComparison.InvariantCultureIgnoreCase);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void LinkJobToSolution(JenkinsJob j)
        {
            try
            {
                string slnPath = ServicesContainer.VisualStudioSolutionInfo.SolutionPath;
                if (string.IsNullOrEmpty(slnPath))
                {
                    Logger.LogInfo(Resources.SolutionNotLoaded);
                    return;
                }

                ApiHandlerSettingsManager.SaveJobForSolution(
                    j.Url,
                    slnPath,
                    SelectedView.Name,
                    SelectedJenkinsServer.Url);

                UpdateJobLinkedStatus();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void ShowWebsite(JenkinsJob j)
        {
            try
            {
                Process.Start(j.Url);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async void BuildJob(JenkinsJob j)
        {
            try
            {
                await BuildJob(j.Url, SelectedJenkinsServer.Url);
                ForceReload(false);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async void ScheduleJob(JenkinsJob j)
        {
            try
            {
                await JenkinsJobManager.ScheduleJob(j.Url, SelectedJenkinsServer.Url);
                ForceReload(false);
            }
            catch (WebException ex)
            {
                Logger.Log(ex);

                var resp = ex.Response as HttpWebResponse;
                if (resp != null)
                {
                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Schedule job", resp.StatusDescription));
                }
                else
                {
                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Schedule job", ex.Status));
                }
            }
        }

        private async void CancelBuild(JenkinsJob j)
        {
            try
            {
                await JenkinsJobManager.CancelBuild(j, SelectedJenkinsServer.Url);
                ForceReload(false);
            }
            catch (WebException ex)
            {
                Logger.Log(ex);

                var resp = ex.Response as HttpWebResponse;
                if (resp != null)
                {
                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Cancel job", resp.StatusDescription));
                }
                else
                {
                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Cancel job", ex.Status));
                }
            }
        }

        private async void DequeueBuild(JenkinsJob j)
        {
            if (j.QueueItem == null)
            {
                return;
            }

            try
            {
                await JenkinsJobManager.DequeueJob(j.QueueItem, SelectedJenkinsServer.Url);
                ForceReload(false);
            }
            catch (WebException ex)
            {
                var resp = ex.Response as HttpWebResponse;
                if (resp != null)
                {
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Known bug (https://issues.jenkins-ci.org/browse/JENKINS-21311), dequeue actually worked but
                        // returns a 404 result.
                        ForceReload(false);
                        return;
                    }

                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Dequeue job", resp.StatusDescription));
                }
                else
                {
                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Dequeue job", ex.Status));
                }

                Logger.Log(ex);
            }
        }

        public async Task BuildJob(string jobUrl, string solutionUrl)
        {
            try
            {
                await JenkinsJobManager.BuildJob(jobUrl, solutionUrl);
            }
            catch (WebException ex)
            {
                Logger.Log(ex);

                var resp = ex.Response as HttpWebResponse;
                if (resp != null)
                {
                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Build job", resp.StatusDescription));
                }
                else
                {
                    Logger.LogInfo(string.Format(Resources.WebExceptionMessage, "Build job", ex.Status));
                }
            }
        }

        private bool CanDoJobAction(JenkinsJob arg)
        {
            return SelectedJob != null;
        }

        public JenkinsJob SelectedJob
        {
            get { return _selectedJob; }
            set
            {
                if (_selectedJob != value)
                {
                    _selectedJob = value;
                    RaisePropertyChanged(() => SelectedJob);
                    BuildJobCommand.RaiseCanExecuteChanged();
                    ShowJobsWebsite.RaiseCanExecuteChanged();
                    LinkJobToCurrentSolution.RaiseCanExecuteChanged();
                    ShowLatestLog.RaiseCanExecuteChanged();
                }
            }
        }

        public JenkinsServer SelectedJenkinsServer
        {
            get { return _selectedJenkinsServer; }
            set
            {
                if (value != _selectedJenkinsServer)
                {
                    SelectNewJenkinsServer(value);
                }
            }
        }

        private void SelectNewJenkinsServer(JenkinsServer value, string preferredViewName = null)
        {
            _selectedJenkinsServer = value;
            RaisePropertyChanged(() => SelectedJenkinsServer);
            RefreshViews(preferredViewName);
        }

        public IEnumerable<JenkinsView> JenkinsViews
        {
            get { return _jenkinsViews; }
            set
            {
                _jenkinsViews = value;
                RaisePropertyChanged(() => JenkinsViews);
            }
        }

        private async void RefreshViews(string preferredViewName)
        {
            try
            {
                if (SelectedJenkinsServer == null)
                {
                    return;
                }

                JenkinsServersEnabled = false;

                JenkinsViews = await JenkinsDataLoader.GetViews(SelectedJenkinsServer);

                var preferredView = JenkinsViews.FirstOrDefault((v) => string.Equals(v.Name, preferredViewName));

                SelectedView = preferredView ?? JenkinsViews.FirstOrDefault();

                ForceReload(true);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void ForceReload(bool disableJenkinsOptions)
        {
            LoadingFailed = false;

            if (disableJenkinsOptions)
            {
                JenkinsServersEnabled = false;
            }

            lock (_loadingJobsBusyLock)
            {
                _refreshTimer.Stop();

                if (_loadingJobsBusy)
                {
                    _forceRefresh = true;
                }
                else
                {
                    _refreshTimer.Interval = 1;
                    _refreshTimer.Start();
                }
            }
        }

        private async Task LoadJenkinsJobs()
        {
            try
            {
                if (SelectedJenkinsServer == null || SelectedView == null)
                {
                    return;
                }

                lock (_loadingJobsBusyLock)
                {
                    if (_loadingJobsBusy)
                    {
                        return;
                    }

                    _loadingJobsBusy = true;
                }

                string sourceUrl = SelectedJenkinsServer.Url;
                IEnumerable<JenkinsJob> refreshedJobs = await JenkinsDataLoader.GetJobsFromView(SelectedJenkinsServer, SelectedView);

                if (TryHandleNewJenkinsOverview(refreshedJobs, sourceUrl))
                {
                    JenkinsServersEnabled = true;
                    UpdateJobLinkedStatus();
                }

                LoadingFailed = false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                LoadingFailed = true;
                UIHelper.InvokeUI(() => Jobs.Clear());
                _refreshTimer.Stop();
                JenkinsServersEnabled = true;
            }
            finally
            {
                lock (_loadingJobsBusyLock)
                {
                    _loadingJobsBusy = false;

                    if (!LoadingFailed)
                    {
                        _refreshTimer.Interval = _forceRefresh ? 1 : _refreshInterval;
                        _forceRefresh = false;
                        _refreshTimer.Start();
                    }
                }
            }
        }

        private bool TryHandleNewJenkinsOverview(IEnumerable<JenkinsJob> refreshedJobs, string sourceUrl)
        {
            if (!string.Equals(sourceUrl, SelectedJenkinsServer.Url))
            {
                return false;
            }

            var prevSelectedJob = SelectedJob;

            var jobsToAdd = refreshedJobs.Except(Jobs);
            var jobsToUpdate = refreshedJobs.Intersect(Jobs);
            var jobsToRemove = Jobs.Except(refreshedJobs).ToArray();

            UIHelper.InvokeUI(() =>
                {
                    foreach (var j in jobsToAdd)
                    {
                        Jobs.Add(j);
                    }
                    foreach (var j in jobsToUpdate)
                    {
                        var refreshedJobOriginal = Jobs.Single((ej) => object.Equals(ej, j));
                        MergeUpdatedJenkinsJob(j, refreshedJobOriginal);
                    }
                    foreach (var j in jobsToRemove)
                    {
                        Jobs.Remove(j);
                    }

                    SelectedJob = Jobs.FirstOrDefault((oj) => object.Equals(oj, prevSelectedJob));
                });

            return true;
        }

        private void MergeUpdatedJenkinsJob(JenkinsJob refreshed, JenkinsJob original)
        {
            original.Builds = refreshed.Builds;
            original.IsEnabled = refreshed.IsEnabled;
            original.IsQueued = refreshed.IsQueued;
            original.Name = refreshed.Name;
            original.QueueItem = refreshed.QueueItem;
        }

        public bool LoadingFailed
        {
            get { return _loadingFailed; }
            set
            {
                if (_loadingFailed != value)
                {
                    _loadingFailed = value;
                    RaisePropertyChanged(() => LoadingFailed);
                }
            }
        }
    }
}
