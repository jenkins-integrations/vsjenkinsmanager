using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.APIHandler.Model;
using Devkoes.JenkinsManagerUI.Helpers;
using Devkoes.JenkinsManagerUI.Managers;
using Devkoes.JenkinsManagerUI.Properties;
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

namespace Devkoes.JenkinsManagerUI.ViewModels
{
    public class JenkinsManagerViewModel : ViewModelBase
    {
        private bool _showAddJenkinsServer;
        private JenkinsServer _selectedJenkinsServer;
        private string _statusMessage;
        private Job _selectedJob;
        private Timer _refreshTimer;
        private bool _loadingJobsBusy;
        private object _loadingJobsBusyLock;
        private bool _loadingFailed;
        private View _selectedView;
        private JenkinsOverview _jOverview;

        public RelayCommand ShowAddJenkinsForm { get; private set; }
        public RelayCommand SaveJenkinsServer { get; private set; }
        public RelayCommand RemoveJenkinsServer { get; private set; }
        public RelayCommand CancelSaveJenkinsServer { get; private set; }
        public RelayCommand Reload { get; private set; }
        public RelayCommand<Job> ScheduleJobCommand { get; private set; }
        public RelayCommand<Job> ShowJobsWebsite { get; private set; }
        public RelayCommand<Job> LinkJobToCurrentSolution { get; private set; }

        public string AddServerUrl { get; set; }
        public string AddServerName { get; set; }
        public string AddUserName { get; set; }
        public string AddAPIToken { get; set; }

        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }

        public JenkinsManagerViewModel()
        {
            ShowAddJenkinsForm = new RelayCommand(HandleShowAddJenkinsServer);
            SaveJenkinsServer = new RelayCommand(HandleSaveJenkinsServer);
            RemoveJenkinsServer = new RelayCommand(HandleRemoveJenkinsServer);
            CancelSaveJenkinsServer = new RelayCommand(HandleCancelSaveJenkinsServer);
            Reload = new RelayCommand(HandleReload);
            ScheduleJobCommand = new RelayCommand<Job>(ScheduleJob, CanDoJobAction);
            ShowJobsWebsite = new RelayCommand<Job>(ShowWebsite, CanDoJobAction);
            LinkJobToCurrentSolution = new RelayCommand<Job>(LinkJobToSolution, CanDoJobAction);
            JenkinsServers = new ObservableCollection<JenkinsServer>();
            _loadingJobsBusyLock = new object();

            SolutionManager.Instance.SolutionPathChanged += SolutionPathChanged;

            LoadJenkinsServers();

            _refreshTimer = new Timer(5000);
            _refreshTimer.Elapsed += RefreshJobsTimerCallback;
            _refreshTimer.Start();
        }

        public View SelectedView
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

        private void SolutionPathChanged(object sender, SolutionPathChangedEventArgs e)
        {
            UpdateJobLinkedStatus(e.SolutionPath);
        }

        private void UpdateJobLinkedStatus(string slnPath = null)
        {
            if (string.IsNullOrEmpty(slnPath))
            {
                slnPath = SolutionManager.Instance.CurrentSolutionPath;
            }

            SolutionJob sJob = SettingManager.GetJobUri(slnPath);

            var allJobs = JOverview.Views.SelectMany((v) => v.Jobs ?? Enumerable.Empty<Job>()).ToArray();

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

        private void LinkJobToSolution(Job j)
        {
            if (string.IsNullOrEmpty(SolutionManager.Instance.CurrentSolutionPath))
            {
                StatusMessage = Resources.SolutionNotLoaded;
                return;
            }

            SettingManager.SaveJobForSolution(j.Url, SolutionManager.Instance.CurrentSolutionPath, SelectedJenkinsServer.Url);

            UpdateJobLinkedStatus();
        }

        private void ShowWebsite(Job j)
        {
            Process.Start(j.Url);
        }

        private async void ScheduleJob(Job j)
        {
            await ScheduleJob(j.Url, SelectedJenkinsServer.Url);
            await LoadJenkinsJobs();
        }

        public async Task ScheduleJob(string jobUrl, string solutionUrl)
        {
            try
            {
                await JenkinsManager.APIHandler.Managers.JenkinsManager.ScheduleJob(jobUrl, solutionUrl);
            }
            catch (WebException ex)
            {
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

        private bool CanDoJobAction(Job arg)
        {
            return SelectedJob != null;
        }

        public Job SelectedJob
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
            SettingManager.RemoveServer(SelectedJenkinsServer);
            LoadJenkinsServers();
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

            lock (_loadingJobsBusyLock)
            {
                if (_loadingJobsBusy)
                    return;

                _loadingJobsBusy = true;
            }

            try
            {
                JenkinsOverview newOverview = await JenkinsManager.APIHandler.Managers.JenkinsManager.GetJenkinsOverview(SelectedJenkinsServer.Url);

                if (JOverview == null)
                {
                    JOverview = newOverview;
                    SelectedView = JOverview.Views.FirstOrDefault();
                }
                else
                {
                    foreach (var newView in newOverview.Views)
                    {
                        var existingView = JOverview.Views.FirstOrDefault((v) => string.Equals(v.Url, newView.Url));
                        if (existingView != null)
                        {
                            var existingJobs = existingView.Jobs;
                            var newJobs = newView.Jobs;

                            IEnumerable<Job> jobsToDelete = existingJobs.Except(newJobs, Job.JobComparer).ToArray();
                            IEnumerable<Job> jobsToAdd = newJobs.Except(existingJobs, Job.JobComparer).ToArray();
                            IEnumerable<Job> jobsToUpdate = newJobs.Intersect(existingJobs, Job.JobComparer).ToArray();

                            UIHelper.InvokeUI(() =>
                                {
                                    foreach (var job in jobsToDelete)
                                    {
                                        existingJobs.Remove(job);
                                    }

                                    foreach (var job in jobsToAdd)
                                    {
                                        existingJobs.Add(job);
                                    }

                                    foreach (var job in jobsToUpdate)
                                    {
                                        var existingJob = existingJobs.Intersect(new[] { job }, Job.JobComparer).Single();
                                        existingJob.Building = job.Building;
                                        existingJob.Color = job.Color;
                                        existingJob.Name = job.Name;
                                        existingJob.Queued = job.Queued;
                                    }
                                });
                        }
                    }
                }
                LoadingFailed = false;

                StatusMessage = null;

                UpdateJobLinkedStatus();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
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

        private void LoadJenkinsServers()
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

        private void HandleSaveJenkinsServer()
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
