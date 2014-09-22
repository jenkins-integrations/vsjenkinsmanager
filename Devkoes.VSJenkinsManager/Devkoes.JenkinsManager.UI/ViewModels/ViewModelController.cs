using System;

namespace Devkoes.JenkinsManagerUI.ViewModels
{
    public static class ViewModelController
    {
        private static Lazy<JenkinsManagerViewModel> _jenkinsManagerViewModel;

        static ViewModelController()
        {
            _jenkinsManagerViewModel = new Lazy<JenkinsManagerViewModel>(() => new JenkinsManagerViewModel());
        }

        public static JenkinsManagerViewModel JenkinsManagerViewModel
        {
            get { return _jenkinsManagerViewModel.Value; }
        }
    }
}
