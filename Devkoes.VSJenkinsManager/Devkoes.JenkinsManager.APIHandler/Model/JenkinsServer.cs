
namespace Devkoes.JenkinsManager.APIHandler.Model
{
    public class JenkinsServer: ObservableObject
    {
        private string _Url;
        private string _Name;
        private string _UserName;
        private string _ApiToken;
        
        public string Url 
        {
            get { return _Url; }
            set
            {
                if (value != _Url)
                {
                    _Url = value;
                    RaisePropertyChanged(() => Url);
                }
            }
        }
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }
        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (value != _UserName)
                {
                    _UserName = value;
                    RaisePropertyChanged(() => UserName);
                }
            }
        }
        public string ApiToken
        {
            get { return _ApiToken; }
            set
            {
                if (value != _ApiToken)
                {
                    _ApiToken = value;
                    RaisePropertyChanged(() => ApiToken);
                }
            }
        }

    }
}
