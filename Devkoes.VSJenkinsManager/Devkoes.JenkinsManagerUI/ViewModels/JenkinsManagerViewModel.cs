using Devkoes.JenkinsClient;
using Devkoes.JenkinsClient.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;

namespace Devkoes.JenkinsManagerUI.ViewModels
{
    internal class JenkinsManagerViewModel : ViewModelBase
    {
        private bool _showAddJenkinsServer;

        public RelayCommand ShowAddJenkinsForm { get; private set; }
        public RelayCommand SaveJenkinsServer { get; private set; }
        public RelayCommand RemoveJenkinsServer { get; private set; }
        public RelayCommand CancelSaveJenkinsServer { get; private set; }

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

            JenkinsServers = new ObservableCollection<JenkinsServer>();
            Jobs = new ObservableCollection<Job>();

            LoadJenkinsServers();
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

        private JenkinsServer _selectedJenkinsServer;

        public JenkinsServer SelectedJenkinsServer
        {
            get { return _selectedJenkinsServer; }
            set
            {
                _selectedJenkinsServer = value;
                LoadJenkinsJobs();
            }
        }

        private async void LoadJenkinsJobs()
        {
            Jobs.Clear();
            if (SelectedJenkinsServer != null)
            {
                var jobs = await JenkinsManager.GetJobs(SelectedJenkinsServer.Url);
                foreach (var job in jobs)
                {
                    Jobs.Add(job);
                }
            }
        }

        private void LoadJenkinsServers()
        {
            JenkinsServers.Clear();
            var servers = JenkinsManager.GetServers();
            foreach (var server in servers)
            {
                if(SelectedJenkinsServer == null)
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

    }
}
