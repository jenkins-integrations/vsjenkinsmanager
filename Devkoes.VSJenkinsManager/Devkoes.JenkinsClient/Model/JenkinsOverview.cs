using System.Collections.Generic;

namespace Devkoes.JenkinsClient.Model
{
    public class Job : ObservableObject
    {
        private bool _building;
        private bool _linkedToCurrentSolution;

        public string Name { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }

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
    }

    public class JenkinsOverview
    {
        public IEnumerable<Job> Jobs { get; set; }
    }
}
