using Devkoes.JenkinsManager.UI.ViewModels;
using System.Windows.Controls;

namespace Devkoes.JenkinsManager.UI.Views
{
    /// <summary>
    /// Interaction logic for JenkinsManager.xaml
    /// </summary>
    public partial class JenkinsManager : UserControl
    {
        public JenkinsManager()
        {
            InitializeComponent();

            DataContext = ViewModelController.JenkinsManagerViewModel;
        }
    }
}
