using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;

namespace Devkoes.JenkinsManager.UI.Views
{
    public class UserOptionsPage : DialogPage
    {
        private static IWin32Window _basicOptions;

        static UserOptionsPage()
        {
            _basicOptions = new UserOptionsControl();
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
