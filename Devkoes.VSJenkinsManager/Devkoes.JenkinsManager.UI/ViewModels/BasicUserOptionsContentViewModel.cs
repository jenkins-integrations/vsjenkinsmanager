using Devkoes.JenkinsManager.APIHandler.Managers;
using GalaSoft.MvvmLight;

namespace Devkoes.JenkinsManager.UI.ViewModels
{
    public class BasicUserOptionsContentViewModel : ViewModelBase
    {
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

    }
}
