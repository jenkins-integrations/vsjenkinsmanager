
namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsJob : ObservableObject
    {
        private bool _building;
        private bool _linkedToCurrentSolution;
        private bool _queued;
        private string _color;

        public string Name { get; set; }
        public string Url { get; set; }
        public string QueuedWhy { get; set; }

        public bool Building
        {
            get { return _building; }
            set
            {
                if (value != _building)
                {
                    _building = value;
                    RaisePropertyChanged(() => Building);
                }
            }
        }

        public string Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    RaisePropertyChanged(() => Color);
                }
            }
        }

        public bool LinkedToCurrentSolution
        {
            get { return _linkedToCurrentSolution; }
            set
            {
                if (value != _linkedToCurrentSolution)
                {
                    _linkedToCurrentSolution = value;
                    RaisePropertyChanged(() => LinkedToCurrentSolution);
                }
            }
        }

        public bool Queued
        {
            get { return _queued; }
            set
            {
                if (value != _queued)
                {
                    _queued = value;
                    RaisePropertyChanged(() => Queued);
                }
            }
        }
    }
}
