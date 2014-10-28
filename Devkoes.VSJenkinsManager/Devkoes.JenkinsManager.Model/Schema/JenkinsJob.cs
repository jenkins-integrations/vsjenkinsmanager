using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsJob : ObservableObject
    {
        private bool _linkedToCurrentSolution;
        private IEnumerable<JenkinsBuild> _builds;
        private bool _isEnabled;
        private bool _isQueued;
        private JenkinsQueueItem _queueItem;
        private string _name;

        [JsonProperty("InQueue")]
        public bool IsQueued
        {
            get { return _isQueued; }
            set
            {
                if (_isQueued != value)
                {
                    _isQueued = value;
                    RaisePropertyChanged(() => IsQueued);
                }
            }
        }

        [JsonProperty("Buildable")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    RaisePropertyChanged(() => IsEnabled);
                    RaisePropertyChanged(() => StatusColor);
                }
            }
        }

        [JsonProperty("Builds")]
        public IEnumerable<JenkinsBuild> Builds
        {
            get { return _builds; }
            set
            {
                if (value != _builds)
                {
                    _builds = value;
                    RaisePropertyChanged(() => Builds);
                    RaisePropertyChanged(() => LatestBuild);
                    RaisePropertyChanged(() => StatusColor);
                    RaisePropertyChanged(() => BuildProgress);
                }
            }
        }

        [JsonProperty("QueueItem")]
        public JenkinsQueueItem QueueItem
        {
            get { return _queueItem; }
            set
            {
                if (_queueItem != value)
                {
                    _queueItem = value;
                    RaisePropertyChanged(() => QueueItem);
                }
            }
        }

        [JsonProperty("Name")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonIgnore]
        public JenkinsBuild LatestBuild
        {
            get
            {
                return Builds == null ? null : Builds.FirstOrDefault();
            }
        }

        [JsonIgnore]
        public bool LinkedToCurrentSolution
        {
            get { return _linkedToCurrentSolution; }
            set
            {
                if (_linkedToCurrentSolution != value)
                {
                    _linkedToCurrentSolution = value;
                    RaisePropertyChanged(() => LinkedToCurrentSolution);
                }
            }
        }

        [JsonIgnore]
        public string StatusColor
        {
            get
            {
                if (LatestBuild != null && LatestBuild.IsBuilding)
                {
                    return "Yellow";
                }
                if (!IsEnabled || LatestBuild == null)
                {
                    return "Gray";
                }
                if (LatestBuild != null && LatestBuild.IsSuccessfull)
                {
                    return "ForestGreen";
                }
                if (LatestBuild != null && !LatestBuild.IsSuccessfull)
                {
                    return "Firebrick";
                }
                return "Transparent";
            }
        }

        [JsonIgnore]
        public short BuildProgress
        {
            get
            {
                double progress = 0;

                if (LatestBuild != null)
                {
                    DateTime start = new DateTime(LatestBuild.Timestamp * 10000).AddYears(1969);

                    var busyTime = (DateTime.UtcNow - start).TotalMilliseconds;

                    progress = (100d / LatestBuild.EstimatedDuration) * busyTime;
                }

                return (short)Math.Max(0, Math.Min(progress, 100));
            }
        }

        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null)
            {
                return false;
            }

            var target = obj as JenkinsJob;
            if (target == null)
            {
                return false;
            }

            return string.Equals(target.Url, this.Url);
        }

        public bool Equals(JenkinsJob target)
        {
            if (target == null)
            {
                return false;
            }

            return string.Equals(target.Url, this.Url);
        }

        public override int GetHashCode()
        {
            if (this.Url == null)
            {
                return 0;
            }

            return this.Url.GetHashCode();
        }
    }
}
