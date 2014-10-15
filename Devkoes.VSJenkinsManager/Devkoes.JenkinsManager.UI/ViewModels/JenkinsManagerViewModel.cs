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
        private IEqualityComparer<JenkinsJob> _jobComparer;
        private bool _jenkinsServersEnabled;
        private bool _forceRefresh;
        private ObservableCollection<JenkinsJob> _jobs;
        private IEnumerable<JenkinsView> _jenkinsViews;

        public RelayCommand ShowSettings { get; private set; }
        public RelayCommand Reload { get; private set; }

        public RelayCommand<JenkinsJob> ScheduleJobCommand { get; private set; }
        public RelayCommand<JenkinsJob> ShowJobsWebsite { get; private set; }
        public RelayCommand<JenkinsJob> LinkJobToCurrentSolution { get; private set; }
        public RelayCommand<JenkinsJob> ShowLatestLog { get; private set; }
        public RelayCommand ShowOutputWindow { get; private set; }

        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }

        public JenkinsManagerViewModel()
        {
            _jobs = new ObservableCollection<JenkinsJob>();
            _jenkinsServersEnabled = true;
            _jobComparer = new JobComparer();

            Reload = new RelayCommand(HandleReload);
            ShowSettings = new RelayCommand(HandleShowSettings);

            ScheduleJobCommand = new RelayCommand<JenkinsJob>(ScheduleJob, CanDoJobAction);
            ShowJobsWebsite = new RelayCommand<JenkinsJob>(ShowWebsite, CanDoJobAction);
            ShowLatestLog = new RelayCommand<JenkinsJob>(HandleShowLatestLog, CanDoJobAction);
            LinkJobToCurrentSolution = new RelayCommand<JenkinsJob>(LinkJobToSolution, CanDoJobAction);
            ShowOutputWindow = new RelayCommand(HandleShowOutputWindow);
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

            bool isSaved = SettingManager.ContainsSolutionPreference(slnPath);
            if (!isSaved)
            {
                return;
            }

            var pref = SettingManager.GetJobLink(slnPath);
            if (!string.Equals(SelectedJenkinsServer.Url, pref.JenkinsServerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            SettingManager.UpdatePreferredView(slnPath, SelectedView.Name);
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
            bool preferredViewSelected = TrySelectPreferredView(e);

            if (!preferredViewSelected)
            {
                // If correct view was already selected, or view doesn't exists, manually update the link icon
                UpdateJobLinkedStatus(e.SolutionPath);
            }
        }

        private bool TrySelectPreferredView(SolutionChangedEventArgs e)
        {
            if (!SettingManager.ContainsSolutionPreference(e.SolutionPath))
            {
                return false;
            }

            var jobLink = SettingManager.GetJobLink(e.SolutionPath);
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

                SolutionJenkinsJobLink sJob = SettingManager.GetJobLink(slnPath);

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
                    StatusMessage = Resources.SolutionNotLoaded;
                    return;
                }

                SettingManager.SaveJobForSolution(
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
                await JenkinsJobManager.ScheduleJob(jobUrl, solutionUrl);
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

            string prevSelectedJob = SelectedJob == null ? null : SelectedJob.Name;

            UIHelper.InvokeUI(() =>
                {
                    Jobs.Clear();
                    foreach (var j in refreshedJobs)
                    {
                        Jobs.Add(j);

                        if (string.Equals(j.Name, prevSelectedJob))
                        {
                            SelectedJob = j;
                        }
                    }
                });

            return true;
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
