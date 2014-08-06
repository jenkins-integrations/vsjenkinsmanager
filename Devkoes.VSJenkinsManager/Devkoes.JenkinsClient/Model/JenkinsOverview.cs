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
        private bool _queued;
        public string _color;

        public static IEqualityComparer<Job> JobComparer;

        public string Name { get; set; }
        public string Url { get; set; }

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

        public string Color { get { return _color; } set {
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

    public class JenkinsOverview
    {
        public IEnumerable<Job> Jobs { get; set; }
    }
}
