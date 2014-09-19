using Devkoes.JenkinsClient.Managers;
using System.Windows.Forms;

namespace Devkoes.JenkinsManagerUI.Views
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
