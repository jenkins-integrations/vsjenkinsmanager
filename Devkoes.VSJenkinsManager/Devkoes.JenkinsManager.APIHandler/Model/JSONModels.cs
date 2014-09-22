using System;
using System.Collections.Generic;
using System.Linq;

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
        public string QueuedWhy { get; set; }

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

    public class JenkinsOverview
    {
        private IEnumerable<Job> _jobs = Enumerable.Empty<Job>();
        private IEnumerable<View> _views = Enumerable.Empty<View>();

        public IEnumerable<Job> Jobs
        {
            get { return _jobs; }
            set { _jobs = value ?? Enumerable.Empty<Job>(); }
        }

        public IEnumerable<View> Views
        {
            get { return _views; }
            set { _views = value ?? Enumerable.Empty<View>(); }
        }
    }

    public class View
    {
        private IList<Job> _jobs = new List<Job>();

        public string Name { get; set; }
        public string Url { get; set; }

        public IList<Job> Jobs
        {
            get { return _jobs; }
            set { _jobs = value ?? new List<Job>(); }
        }
    }

    public class JenkinsView
    {
        private IEnumerable<Job> _jobs = Enumerable.Empty<Job>();

        public string Name { get; set; }
        public string Url { get; set; }

        public IEnumerable<Job> Jobs
        {
            get { return _jobs; }
            set { _jobs = value ?? Enumerable.Empty<Job>(); }
        }
    }

}
