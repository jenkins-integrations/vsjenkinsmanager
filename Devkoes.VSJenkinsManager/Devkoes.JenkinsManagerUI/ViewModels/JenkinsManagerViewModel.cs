using Devkoes.JenkinsClient;
using Devkoes.JenkinsClient.Model;
using Devkoes.JenkinsManagerUI.Helpers;
using Devkoes.JenkinsManagerUI.Managers;
using Devkoes.JenkinsManagerUI.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
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
        private bool _loadingFailed;

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

        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }
        public ObservableCollection<Job> Jobs { get; private set; }

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
            Jobs = new ObservableCollection<Job>();

            SolutionManager.Instance.SolutionPathChanged += SolutionPathChanged;

            LoadJenkinsServers();

            _refreshTimer = new Timer(5000);
            _refreshTimer.Elapsed += RefreshJobsTimerCallback;
            _refreshTimer.Start();
        }

        private async void HandleReload()
        {
            await LoadJenkinsJobs();
        }

        private async void RefreshJobsTimerCallback(object sender, ElapsedEventArgs e)
        {
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

            string jobUrl = SettingManager.GetJobUri(slnPath);

            UIHelper.InvokeUI(() =>
            {
                foreach (var job in Jobs)
                {
                    job.LinkedToCurrentSolution = string.Equals(job.Url, jobUrl, System.StringComparison.InvariantCultureIgnoreCase);
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

            SettingManager.SaveJobForSolution(j.Url, SolutionManager.Instance.CurrentSolutionPath);

            UpdateJobLinkedStatus();
        }

        private void ShowWebsite(Job j)
        {
            Process.Start(j.Url);
        }

        private async void ScheduleJob(Job j)
        {
            await ScheduleJob(j.Url);
        }

        public async Task ScheduleJob(string jobUrl)
        {
            try
            {
                await JenkinsManager.ScheduleJob(jobUrl);
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
        }

        private void HandleRemoveJenkinsServer()
        {
            JenkinsManager.RemoveServer(SelectedJenkinsServer);
            LoadJenkinsServers();
        }


        public JenkinsServer SelectedJenkinsServer
        {
            get { return _selectedJenkinsServer; }
            set
            {
                _selectedJenkinsServer = value;
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
            if (_loadingJobsBusy)
                return;

            if (SelectedJenkinsServer == null)
                return;

            _loadingJobsBusy = true;

            try
            {
                var newJobs = await JenkinsManager.GetJobs(SelectedJenkinsServer.Url);

                var jobsToDelete = Jobs.Except(newJobs, Job.JobComparer).ToArray();
                var jobsToAdd = newJobs.Except(Jobs, Job.JobComparer).ToArray();
                var jobsToUpdate = newJobs.Intersect(Jobs, Job.JobComparer).ToArray();

                UIHelper.InvokeUI(() =>
                    {
                        foreach (var job in jobsToDelete)
                        {
                            Jobs.Remove(job);
                        }

                        foreach (var job in jobsToAdd)
                        {
                            Jobs.Add(job);
                        }

                        foreach (var job in jobsToUpdate)
                        {
                            var existingJob = Jobs.Intersect(new[] { job }, Job.JobComparer).Single();
                            existingJob.Building = job.Building;
                            existingJob.Color = job.Color;
                            existingJob.Name = job.Name;
                            existingJob.Queued = job.Queued;
                        }
                    });

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
                _loadingJobsBusy = false;
            }
        }

        private void LoadJenkinsServers()
        {
            JenkinsServers.Clear();
            var servers = JenkinsManager.GetServers();
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
            JenkinsManager.AddServer(new JenkinsServer() { Name = AddServerName, Url = AddServerUrl });
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
