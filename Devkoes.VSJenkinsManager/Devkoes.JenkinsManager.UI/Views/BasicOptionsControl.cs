using Devkoes.JenkinsManager.APIHandler.Managers;
using System.Windows.Forms;

namespace Devkoes.JenkinsManager.UI.Views
{
    public partial class BasicOptionsControl : UserControl
    {
        public BasicOptionsControl()
        {
            InitializeComponent();

            debugEnabled.Checked = SettingManager.DebugEnabled;
        }

        private void debugEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            SettingManager.DebugEnabled = debugEnabled.Checked;
        }
    }
}
