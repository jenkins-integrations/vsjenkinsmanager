using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Schema;
using Devkoes.JenkinsManager.UI.ValidationRules;
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
        private JenkinsServer _editJenkinsServer;

        public BasicUserOptionsContentViewModel()
        {
            _editJenkinsServer = new JenkinsServer();

            AddServer = new RelayCommand(HandleAddJenkinsServer);
            RemoveServer = new RelayCommand(HandleRemoveJenkinsServer);
            ApplyChanges = new RelayCommand(HandleApplyChanges);

            JenkinsServers = SettingManager.GetServers();
            SelectedJenkinsServer = JenkinsServers.FirstOrDefault();

            InitializeValidationRules();
        }

        private void InitializeValidationRules()
        {
            _editJenkinsServer.RegisterValidationRule(
                (c) => c.Url,
                (j) => PropertyRequired.Validate("Url", j.Url));

            _editJenkinsServer.RegisterValidationRule(
                (c) => c.Name,
                (j) => PropertyRequired.Validate("Name", j.Name));

            _editJenkinsServer.RegisterValidationRule(
                (c) => c.ApiToken,
                JenkinsServerApiTokenRequired.Validate);

            _editJenkinsServer.RegisterValidationRule(
                (c) => c.UserName,
                JenkinsServerResetApiToken.Validate);

            _editJenkinsServer.RegisterAsyncValidationRule(
                (c) => c.Url,
                JenkinsServerVersion.Validate);
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
            SettingManager.UpdateServer(SelectedJenkinsServer, EditJenkinsServer);
        }

        private void HandleAddJenkinsServer()
        {
            var newJenkinsServer = new JenkinsServer() { Name = "New server", Url = "http://" };

            SettingManager.AddServer(newJenkinsServer);

            SelectedJenkinsServer = newJenkinsServer;

            UpdateEditJenkinsServer();
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
                    UpdateEditJenkinsServer();
                    RaisePropertyChanged(() => SelectedJenkinsServer);
                }
            }
        }

        private void UpdateEditJenkinsServer()
        {
            JenkinsServer server = SelectedJenkinsServer;
            ;
            if (SelectedJenkinsServer == null)
            {
                server = new JenkinsServer();
            }

            EditJenkinsServer.Name = server.Name;
            EditJenkinsServer.Url = server.Url;
            EditJenkinsServer.UserName = server.UserName;
            EditJenkinsServer.ApiToken = server.ApiToken;
        }

        public JenkinsServer EditJenkinsServer
        {
            get { return _editJenkinsServer; }
        }
    }
}
