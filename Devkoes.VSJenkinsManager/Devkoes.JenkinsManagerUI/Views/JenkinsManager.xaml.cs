using Devkoes.JenkinsManagerUI.ViewModels;
using System.Windows.Controls;

namespace Devkoes.JenkinsManagerUI.Views
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
