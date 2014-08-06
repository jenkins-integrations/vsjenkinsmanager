using System;
using System.Collections.Generic;

namespace Devkoes.JenkinsClient.Model
{
    public class JobComparer : IEqualityComparer<Job>
    {
        public bool Equals(Job x, Job y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x != null && y != null)
            {
                return string.Equals(x.Url, y.Url, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public int GetHashCode(Job obj)
        {
            return obj.Url.ToLower().GetHashCode();
        }
    }
    public class Job : ObservableObject
    {
        private bool _building;
        private bool _linkedToCurrentSolution;

        public static IEqualityComparer<Job> JobComparer;

        public string Name { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }

        static Job()
        {
            JobComparer = new JobComparer();
        }

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
