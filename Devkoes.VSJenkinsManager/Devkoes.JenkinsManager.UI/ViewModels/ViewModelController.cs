using System;

namespace Devkoes.JenkinsManager.UI.ViewModels
{
    public static class ViewModelController
    {
        private static Lazy<JenkinsManagerViewModel> _jenkinsManagerViewModel;
        private static Lazy<BasicUserOptionsContentViewModel> _basicUserOptionsContentViewModel;

        static ViewModelController()
        {
            _jenkinsManagerViewModel = new Lazy<JenkinsManagerViewModel>(() => new JenkinsManagerViewModel());
            _basicUserOptionsContentViewModel = new Lazy<BasicUserOptionsContentViewModel>(() => new BasicUserOptionsContentViewModel());
        }

        public static JenkinsManagerViewModel JenkinsManagerViewModel
        {
            get { return _jenkinsManagerViewModel.Value; }
        }

        public static BasicUserOptionsContentViewModel BasicUserOptionsContentViewModel
        {
            get { return _basicUserOptionsContentViewModel.Value; }
        }
    }
}
