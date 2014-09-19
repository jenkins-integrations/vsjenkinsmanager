using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;

namespace Devkoes.JenkinsManagerUI.Views
{
    public class BasicOptionsPage : DialogPage
    {
        private static IWin32Window _basicOptions;

        static BasicOptionsPage()
        {
            _basicOptions = new BasicOptionsControl();
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
