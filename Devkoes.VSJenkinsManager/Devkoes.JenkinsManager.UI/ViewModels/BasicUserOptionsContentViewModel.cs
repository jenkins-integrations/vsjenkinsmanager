using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Schema;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;

namespace Devkoes.JenkinsManager.UI.ViewModels
{
    public class BasicUserOptionsContentViewModel : ViewModelBase
    {
        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }
        public RelayCommand AddServer { get; private set; }
        private JenkinsServer _selectedJenkinsServer;

        public BasicUserOptionsContentViewModel()
        {
            JenkinsServers = new ObservableCollection<JenkinsServer>();
            AddServer = new RelayCommand(HandleAddJenkinsServer);

            LoadJenkinsServers();
        }

        private void HandleAddJenkinsServer()
        {
            var newServer = new JenkinsServer();
            JenkinsServers.Add(newServer);
            SelectedJenkinsServer = newServer;
        }

        public bool DebugEnabled
        {
            get { return SettingManager.DebugEnabled; }
            set
            {
                if (SettingManager.DebugEnabled != value)
                {
                    SettingManager.DebugEnabled = value;
                    RaisePropertyChanged(() => DebugEnabled);
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
    }
}
