using Microsoft.VisualStudio.Shell;

namespace Devkoes.JenkinsManager.UI.Views
{
    public class UserOptionsHost : UIElementDialogPage
    {
        private static BasicUserOptionsContent _basicOptions;

        static UserOptionsHost()
        {
            _basicOptions = new BasicUserOptionsContent();
        }

        protected override System.Windows.UIElement Child
        {
            get { return _basicOptions; }
        }
    }
}
