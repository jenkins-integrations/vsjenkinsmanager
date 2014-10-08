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
using System.IO;
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
        private string _statusMessage;
        private JenkinsJob _selectedJob;
        private Timer _refreshTimer;
        private bool _loadingJobsBusy;
        private object _loadingJobsBusyLock;
        private bool _loadingFailed;
        private JenkinsView _selectedView;
        private JenkinsOverview _jOverview;
        private IEqualityComparer<JenkinsJob> _jobComparer;
        private bool _jenkinsServersEnabled;
        private bool _forceRefresh;

        public RelayCommand ShowSettings { get; private set; }
        public RelayCommand Reload { get; private set; }

        public RelayCommand<JenkinsJob> ScheduleJobCommand { get; private set; }
        public RelayCommand<JenkinsJob> ShowJobsWebsite { get; private set; }
        public RelayCommand<JenkinsJob> LinkJobToCurrentSolution { get; private set; }
        public RelayCommand<JenkinsJob> ShowLatestLog { get; private set; }

        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }

        public JenkinsManagerViewModel()
        {
            _jenkinsServersEnabled = true;
            _jobComparer = new JobComparer();

            Reload = new RelayCommand(HandleReload);
            ShowSettings = new RelayCommand(HandleShowSettings);

            ScheduleJobCommand = new RelayCommand<JenkinsJob>(ScheduleJob, CanDoJobAction);
            ShowJobsWebsite = new RelayCommand<JenkinsJob>(ShowWebsite, CanDoJobAction);
            ShowLatestLog = new RelayCommand<JenkinsJob>(HandleShowLatestLog, CanDoJobAction);
            LinkJobToCurrentSolution = new RelayCommand<JenkinsJob>(LinkJobToSolution, CanDoJobAction);
            _loadingJobsBusyLock = new object();

            ServicesContainer.VisualStudioSolutionEvents.SolutionChanged += SolutionPathChanged;

            _refreshTimer = new Timer(_refreshInterval);
            _refreshTimer.Elapsed += RefreshJobsTimerCallback;
            _refreshTimer.AutoReset = false;

            JenkinsServers = SettingManager.GetServers();
            SelectedJenkinsServer = JenkinsServers.FirstOrDefault();

            _refreshTimer.Start();
        }

        private void HandleShowLatestLog(JenkinsJob job)
        {
            try
            {
                // TODO: move webclient stuff elsewhere
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
                WebClient wc = new WebClient();
                string b = wc.DownloadString(job.Url + "/lastBuild/consoleText");

                File.WriteAllText(fileName, b);

                ServicesContainer.VisualStudioFileManager.OpenFile(fileName);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void HandleShowSettings()
        {
            ServicesContainer.VisualStudioWindowHandler.ShowSettingsWindow();
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
                }
            }
        }

        public JenkinsOverview JOverview
        {
            get { return _jOverview; }
            set
            {
                if (value != _jOverview)
                {
                    _jOverview = value;
                }
                RaisePropertyChanged(() => JOverview);
            }
        }

        private void HandleReload()
        {
            ForceReload(false);
        }

        private async void RefreshJobsTimerCallback(object sender, ElapsedEventArgs e)
        {
            await LoadJenkinsJobs();
        }

        private void SolutionPathChanged(object sender, SolutionChangedEventArgs e)
        {
            UpdateJobLinkedStatus(e.SolutionPath);
        }

        private void UpdateJobLinkedStatus(string slnPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(slnPath))
                {
                    slnPath = ServicesContainer.VisualStudioSolutionInfo.SolutionPath;
                }

                SolutionJenkinsJobLink sJob = SettingManager.GetJobUri(slnPath);

                IEnumerable<JenkinsJob> allJobs;
                if (JOverview == null)
                {
                    allJobs = Enumerable.Empty<JenkinsJob>();
                }
                else
                {
                    allJobs = JOverview.Views.SelectMany((v) => v.Jobs ?? Enumerable.Empty<JenkinsJob>()).ToArray();
                }

                UIHelper.InvokeUI(() =>
                {
                    foreach (var job in allJobs)
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
                    StatusMessage = Resources.SolutionNotLoaded;
                    return;
                }

                SettingManager.SaveJobForSolution(j.Url, slnPath, SelectedJenkinsServer.Url);

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

        private async void ScheduleJob(JenkinsJob j)
        {
            try
            {
                await ScheduleJob(j.Url, SelectedJenkinsServer.Url);
                ForceReload(false);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task ScheduleJob(string jobUrl, string solutionUrl)
        {
            try
            {
                await JenkinsManager.APIHandler.Managers.JenkinsManager.ScheduleJob(jobUrl, solutionUrl);
            }
            catch (WebException ex)
            {
                Logger.Log(ex);

                var resp = ex.Response as HttpWebResponse;
                if (resp != null)
                {
                    StatusMessage = string.Format(Resources.WebExceptionMessage, "Schedule job", resp.StatusDescription);
                }
                else
                {
                    StatusMessage = string.Format(Resources.WebExceptionMessage, "Schedule job", ex.Status);
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
                    ScheduleJobCommand.RaiseCanExecuteChanged();
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
                _selectedJenkinsServer = value;
                RaisePropertyChanged(() => SelectedJenkinsServer);
                ForceReload(true);
            }
        }

        private void ForceReload(bool newServerSelected)
        {
            if (newServerSelected)
            {
                JenkinsServersEnabled = false;
            }

            lock (_loadingJobsBusyLock)
            {
                if (newServerSelected)
                {
                    JOverview = null;
                }

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

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                RaisePropertyChanged(() => StatusMessage);
            }
        }

        private async Task LoadJenkinsJobs()
        {
            try
            {
                if (SelectedJenkinsServer == null)
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
                JenkinsOverview newOverview = await JenkinsManager.APIHandler.Managers.JenkinsManager.GetJenkinsOverview(sourceUrl);

                if (TryHandleNewJenkinsOverview(newOverview, sourceUrl))
                {
                    JenkinsServersEnabled = true;
                    UpdateJobLinkedStatus();
                }

                LoadingFailed = false;
                StatusMessage = null;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                Logger.Log(ex);
                LoadingFailed = true;
                _refreshTimer.Stop();
            }
            finally
            {
                lock (_loadingJobsBusyLock)
                {
                    _loadingJobsBusy = false;

                    if (!LoadingFailed)
                    {
                        _refreshTimer.Interval = _forceRefresh ? 1 : _refreshInterval;
                        _refreshTimer.Start();
                    }
                }
            }
        }

        private bool TryHandleNewJenkinsOverview(JenkinsOverview newOverview, string sourceUrl)
        {
            if (!string.Equals(sourceUrl, SelectedJenkinsServer.Url))
            {
                return false;
            }

            if (JOverview == null)
            {
                JOverview = newOverview;
                SelectedView = JOverview.Views.FirstOrDefault();
                return true;
            }

            foreach (var newView in newOverview.Views)
            {
                var existingView = JOverview.Views.FirstOrDefault((v) => string.Equals(v.Url, newView.Url));
                if (existingView != null)
                {
                    var existingJobs = existingView.Jobs;
                    var newJobs = newView.Jobs;

                    IEnumerable<JenkinsJob> jobsToDelete = existingJobs.Except(newJobs, _jobComparer).ToArray();
                    IEnumerable<JenkinsJob> jobsToAdd = newJobs.Except(existingJobs, _jobComparer).ToArray();
                    IEnumerable<JenkinsJob> jobsToUpdate = newJobs.Intersect(existingJobs, _jobComparer).ToArray();

                    UIHelper.InvokeUI(() =>
                    {
                        DeleteJobs(existingJobs, jobsToDelete);
                        AddJobs(existingJobs, jobsToAdd);
                        ModifyJobs(existingJobs, jobsToUpdate);
                    });
                }
            }

            return true;
        }

        private void ModifyJobs(IList<JenkinsJob> existingJobs, IEnumerable<JenkinsJob> jobsToUpdate)
        {
            foreach (var job in jobsToUpdate)
            {
                var existingJob = existingJobs.Intersect(new[] { job }, _jobComparer).Single();
                existingJob.Building = job.Building;
                existingJob.Color = job.Color;
                existingJob.Name = job.Name;
                existingJob.Queued = job.Queued;
            }
        }

        private static void AddJobs(IList<JenkinsJob> existingJobs, IEnumerable<JenkinsJob> jobsToAdd)
        {
            foreach (var job in jobsToAdd)
            {
                existingJobs.Add(job);
            }
        }

        private static void DeleteJobs(IList<JenkinsJob> existingJobs, IEnumerable<JenkinsJob> jobsToDelete)
        {
            foreach (var job in jobsToDelete)
            {
                existingJobs.Remove(job);
            }
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
