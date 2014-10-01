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
        private bool _showAddJenkinsServer;
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

        public RelayCommand ShowAddJenkinsForm { get; private set; }
        public RelayCommand SaveJenkinsServer { get; private set; }
        public RelayCommand RemoveJenkinsServer { get; private set; }
        public RelayCommand CancelSaveJenkinsServer { get; private set; }
        public RelayCommand Reload { get; private set; }
        public RelayCommand<JenkinsJob> ScheduleJobCommand { get; private set; }
        public RelayCommand<JenkinsJob> ShowJobsWebsite { get; private set; }
        public RelayCommand<JenkinsJob> LinkJobToCurrentSolution { get; private set; }

        public string AddServerUrl { get; set; }
        public string AddServerName { get; set; }
        public string AddUserName { get; set; }
        public string AddAPIToken { get; set; }

        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }

        public JenkinsManagerViewModel()
        {
            _jobComparer = new JobComparer();
            ShowAddJenkinsForm = new RelayCommand(HandleShowAddJenkinsServer);
            SaveJenkinsServer = new RelayCommand(HandleSaveJenkinsServer);
            RemoveJenkinsServer = new RelayCommand(HandleRemoveJenkinsServer);
            CancelSaveJenkinsServer = new RelayCommand(HandleCancelSaveJenkinsServer);
            Reload = new RelayCommand(HandleReload);
            ScheduleJobCommand = new RelayCommand<JenkinsJob>(ScheduleJob, CanDoJobAction);
            ShowJobsWebsite = new RelayCommand<JenkinsJob>(ShowWebsite, CanDoJobAction);
            LinkJobToCurrentSolution = new RelayCommand<JenkinsJob>(LinkJobToSolution, CanDoJobAction);
            JenkinsServers = new ObservableCollection<JenkinsServer>();
            _loadingJobsBusyLock = new object();

            ServicesContainer.VisualStudioSolutionEvents.SolutionChanged += SolutionPathChanged;

            LoadJenkinsServers();

            _refreshTimer = new Timer(5000);
            _refreshTimer.Elapsed += RefreshJobsTimerCallback;
            _refreshTimer.Start();
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

        private async void HandleReload()
        {
            await LoadJenkinsJobs();
        }

        private async void RefreshJobsTimerCallback(object sender, ElapsedEventArgs e)
        {
            lock (_loadingJobsBusyLock)
            {
                if (_loadingJobsBusy)
                {
                    return;
                }
            }

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

                var allJobs = JOverview.Views.SelectMany((v) => v.Jobs ?? Enumerable.Empty<JenkinsJob>()).ToArray();

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
                await LoadJenkinsJobs();
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
                }
            }
        }

        private void HandleCancelSaveJenkinsServer()
        {
            ShowAddJenkinsServer = false;
            AddServerName = null;
            AddServerUrl = null;
            AddUserName = null;
            AddAPIToken = null;
        }

        private void HandleRemoveJenkinsServer()
        {
            try
            {
                SettingManager.RemoveServer(SelectedJenkinsServer);
                LoadJenkinsServers();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public JenkinsServer SelectedJenkinsServer
        {
            get { return _selectedJenkinsServer; }
            set
            {
                _selectedJenkinsServer = value;
                JOverview = null;
                RaisePropertyChanged(() => SelectedJenkinsServer);
                LoadJenkinsJobs();
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
            if (SelectedJenkinsServer == null)
                return;

            string sourceUrl = SelectedJenkinsServer.Url;

            lock (_loadingJobsBusyLock)
            {
                if (_loadingJobsBusy)
                    return;

                _loadingJobsBusy = true;
            }

            try
            {
                JenkinsOverview newOverview = await JenkinsManager.APIHandler.Managers.JenkinsManager.GetJenkinsOverview(sourceUrl);

                HandleNewJenkinsOverview(newOverview, sourceUrl);

                LoadingFailed = false;
                StatusMessage = null;
                UpdateJobLinkedStatus();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                Logger.Log(ex);
                LoadingFailed = true;
            }
            finally
            {
                lock (_loadingJobsBusyLock)
                {
                    _loadingJobsBusy = false;
                }
            }
        }

        private void HandleNewJenkinsOverview(JenkinsOverview newOverview, string sourceUrl)
        {
            if (SelectedJenkinsServer == null || !string.Equals(SelectedJenkinsServer.Url, sourceUrl))
            {
                // In the time we were loading the data, the selected server could be changed. If done so,
                // ignore this data for it is invalid.
                return;
            }

            if (JOverview == null)
            {
                JOverview = newOverview;
                SelectedView = JOverview.Views.FirstOrDefault();
                return;
            }

            MergeJenkinsOverviewData(newOverview);
        }

        private void MergeJenkinsOverviewData(JenkinsOverview newOverview)
        {
            foreach (var newView in newOverview.Views)
            {
                var existingView = JOverview.Views.FirstOrDefault((v) => string.Equals(v.Url, newView.Url));
                if (existingView != null)
                {
                    var existingJobs = existingView.Jobs;
                    var newJobs = newView.Jobs;

                    IEnumerable<JenkinsJob> jobsToAdd = newJobs.Except(existingJobs, _jobComparer).ToArray();
                    IEnumerable<JenkinsJob> jobsToDelete = existingJobs.Except(newJobs, _jobComparer).ToArray();
                    IEnumerable<JenkinsJob> jobsToUpdate = newJobs.Intersect(existingJobs, _jobComparer).ToArray();

                    UIHelper.InvokeUI(() =>
                    {
                        AddNewJob(existingJobs, jobsToAdd);
                        DeleteJob(existingJobs, jobsToDelete);
                        UpdateJob(existingJobs, jobsToUpdate);
                    });
                }
            }
        }

        private void AddNewJob(IList<JenkinsJob> existingJobs, IEnumerable<JenkinsJob> jobsToAdd)
        {
            foreach (var job in jobsToAdd)
            {
                existingJobs.Add(job);
            }
        }

        private void DeleteJob(IList<JenkinsJob> existingJobs, IEnumerable<JenkinsJob> jobsToDelete)
        {
            foreach (var job in jobsToDelete)
            {
                existingJobs.Remove(job);
            }
        }

        private void UpdateJob(IList<JenkinsJob> existingJobs, IEnumerable<JenkinsJob> jobsToUpdate)
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

        private void LoadJenkinsServers()
        {
            try
            {
                JenkinsServers.Clear();
                var servers = SettingManager.GetServers();
                foreach (var server in servers)
                {
                    if (SelectedJenkinsServer == null)
                    {
                        SelectedJenkinsServer = server;
                    }
                    JenkinsServers.Add(server);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void HandleSaveJenkinsServer()
        {
            try
            {
                SettingManager.AddServer(new JenkinsServer()
                    {
                        Name = AddServerName,
                        Url = AddServerUrl,
                        UserName = AddUserName,
                        ApiToken = AddAPIToken
                    });

                LoadJenkinsServers();
                ShowAddJenkinsServer = false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void HandleShowAddJenkinsServer()
        {
            ShowAddJenkinsServer = !ShowAddJenkinsServer;
        }

        public bool ShowAddJenkinsServer
        {
            get { return _showAddJenkinsServer; }
            set
            {
                _showAddJenkinsServer = value;
                RaisePropertyChanged(() => ShowAddJenkinsServer);
            }
        }

        public bool LoadingFailed
        {
            get { return _loadingFailed; }
            set
            {
                if (_loadingFailed != value)
                {
                    if (value)
                    {
                        _refreshTimer.Stop();
                    }
                    else
                    {
                        _refreshTimer.Start();
                    }

                    _loadingFailed = value;
                    RaisePropertyChanged(() => LoadingFailed);
                }
            }
        }
    }
}
