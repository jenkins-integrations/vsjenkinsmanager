using Devkoes.JenkinsManager.UI.ViewModels;
using System.Windows.Controls;

namespace Devkoes.JenkinsManager.UI.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BasicUserOptionsContent : UserControl
    {
        public BasicUserOptionsContent()
        {
            InitializeComponent();

            DataContext = ViewModelController.BasicUserOptionsContentViewModel;
        }
    }
}
