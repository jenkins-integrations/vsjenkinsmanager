using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Schema;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Linq;

namespace Devkoes.JenkinsManager.UI.ViewModels
{
    public class BasicUserOptionsContentViewModel : ViewModelBase
    {
        private enum ModifyMode { None, Add, Edit }

        public ObservableCollection<JenkinsServer> JenkinsServers { get; private set; }
        public RelayCommand AddServer { get; private set; }
        public RelayCommand RemoveServer { get; private set; }
        public RelayCommand ApplyChanges { get; private set; }
        private JenkinsServer _selectedJenkinsServer;
        private JenkinsServer _addEditJenkinsServer;
        private ModifyMode _currentModifyMode;

        public BasicUserOptionsContentViewModel()
        {
            _addEditJenkinsServer = new JenkinsServer();
            AddServer = new RelayCommand(HandleAddJenkinsServer);
            RemoveServer = new RelayCommand(HandleRemoveJenkinsServer);
            ApplyChanges = new RelayCommand(HandleApplyChanges);

            JenkinsServers = SettingManager.GetServers();
            SelectedJenkinsServer = JenkinsServers.FirstOrDefault();
        }

        public string RequiredVersion
        {
            get
            {
                return JenkinsServerValidator.MINIMUM_VERSION.ToString();
            }
        }

        private void HandleRemoveJenkinsServer()
        {
            if (SelectedJenkinsServer == null)
            {
                return;
            }

            SettingManager.RemoveServer(SelectedJenkinsServer);
        }

        private void HandleApplyChanges()
        {
            if (_currentModifyMode == ModifyMode.Edit)
            {
                SettingManager.UpdateServer(SelectedJenkinsServer, AddEditJenkinsServer);
            }
            else
            {
                if (!JenkinsServerValidator.ValidJenkinsServer(AddEditJenkinsServer.Url))
                {
                    return;
                }

                // We shouldn't add the _addEditJenkinsServer, cause that will add the instance
                // to the list of JenkinsServers. Which we want to prevent, because the _addEditJenkinsServer
                // is a decoupled object to support edit and add of any jenkins server.
                var newJenkinsServer = new JenkinsServer()
                {
                    Name = AddEditJenkinsServer.Name,
                    Url = AddEditJenkinsServer.Url,
                    UserName = AddEditJenkinsServer.UserName,
                    ApiToken = AddEditJenkinsServer.ApiToken
                };

                SettingManager.AddServer(newJenkinsServer);
            }
        }

        private void HandleAddJenkinsServer()
        {
            // will set _currentModifyMode to edit, so do this first
            SelectedJenkinsServer = null;

            _currentModifyMode = ModifyMode.Add;

            UpdateAddEditJenkinsServer();
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
                if (_selectedJenkinsServer != value)
                {
                    _selectedJenkinsServer = value;
                    _currentModifyMode = ModifyMode.Edit;
                    UpdateAddEditJenkinsServer(_selectedJenkinsServer);
                    RaisePropertyChanged(() => SelectedJenkinsServer);
                }
            }
        }

        private void UpdateAddEditJenkinsServer(JenkinsServer server = null)
        {
            if (server == null)
            {
                server = new JenkinsServer() { Name = "New server", Url = "http://" };
            }

            AddEditJenkinsServer.Name = server.Name;
            AddEditJenkinsServer.Url = server.Url;
            AddEditJenkinsServer.UserName = server.UserName;
            AddEditJenkinsServer.ApiToken = server.ApiToken;
        }

        public JenkinsServer AddEditJenkinsServer
        {
            get { return _addEditJenkinsServer; }
        }
    }
}
