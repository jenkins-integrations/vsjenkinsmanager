using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;

namespace Devkoes.JenkinsManager.UI.Views
{
    public class UserOptionsHost : DialogPage
    {
        private static IWin32Window _basicOptions;

        static UserOptionsHost()
        {
            _basicOptions = new BasicUserOptions();
        }

        [BrowsableAttribute(false)]
        protected override IWin32Window Window
        {
            get
            {
                return _basicOptions;
            }
        }

    }
}
