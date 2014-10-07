namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsServer : ObservableObject
    {
        private string _name;
        private string _userName;
        private string _url;
        private string _apiToken;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    RaisePropertyChanged(() => Url);
                }
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    RaisePropertyChanged(() => UserName);
                }
            }
        }

        public string ApiToken
        {
            get { return _apiToken; }
            set
            {
                if (_apiToken != value)
                {
                    _apiToken = value;
                    RaisePropertyChanged(() => ApiToken);
                }
            }
        }
    }
}
