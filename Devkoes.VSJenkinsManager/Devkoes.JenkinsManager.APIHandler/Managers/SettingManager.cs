using Devkoes.JenkinsManager.APIHandler.Properties;
using Devkoes.JenkinsManager.Model.Schema;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    public static class SettingManager
    {
        private static ObservableCollection<JenkinsServer> _serversCopy;

        static SettingManager()
        {
            _serversCopy = new ObservableCollection<JenkinsServer>();

            if (Settings.Default.SolutionJobs == null)
            {
                Settings.Default.SolutionJobs = new SolutionJenkinsJobLinkList();
            }

            if (Settings.Default.JenkinsServers != null)
            {
                foreach (var s in Settings.Default.JenkinsServers)
                {
                    s.Version = JenkinsServerValidator.GetJenkinsVersion(s.Url);
                    _serversCopy.Add(s);
                }
            }

            Settings.Default.Save();
        }

        public static void SaveJobForSolution(
            string jobUri, 
            string solutionPath, 
            string jenkinsViewName,
            string jenkinsServerUri)
        {
            var existingSolutionJob = Settings.Default.SolutionJobs.FirstOrDefault((sj) => string.Equals(sj.SolutionPath, solutionPath, StringComparison.InvariantCultureIgnoreCase));
            if (existingSolutionJob != null)
            {
                Settings.Default.SolutionJobs.Remove(existingSolutionJob);
            }

            Settings.Default.SolutionJobs.Add(new SolutionJenkinsJobLink()
            {
                SolutionPath = solutionPath,
                JobUrl = jobUri,
                JenkinsViewName = jenkinsViewName,
                JenkinsServerUrl = jenkinsServerUri
            });

            Settings.Default.Save();
        }

        public static SolutionJenkinsJobLink GetJobLink(string solutionPath)
        {
            return Settings.Default.SolutionJobs.FirstOrDefault((sj) => string.Equals(sj.SolutionPath, solutionPath, StringComparison.InvariantCultureIgnoreCase));
        }

        public static JenkinsServer GetJenkinsServer(string solutionUrl)
        {
            return _serversCopy.FirstOrDefault((js) => string.Equals(js.Url, solutionUrl, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool ContainsSolutionPreference(string solutionPath)
        {
            return Settings.Default.SolutionJobs.Any((sj) => string.Equals(sj.SolutionPath, solutionPath, StringComparison.InvariantCultureIgnoreCase));
        }

        public static void AddServer(JenkinsServer server)
        {
            _serversCopy.Add(server);
            SaveJenkinsServers();
        }

        public static ObservableCollection<JenkinsServer> GetServers()
        {
            return _serversCopy;
        }

        public static void RemoveServer(JenkinsServer server)
        {
            _serversCopy.Remove(server);
            SaveJenkinsServers();
        }

        public static bool DebugEnabled
        {
            get { return Settings.Default.DebugOutputEnabled; }
            set
            {
                if (value != Settings.Default.DebugOutputEnabled)
                {
                    Settings.Default.DebugOutputEnabled = value;
                    Settings.Default.Save();
                }
            }
        }

        public static void UpdateServer(
            JenkinsServer originalJenkinsServer,
            JenkinsServer newJenkinsServer)
        {
            if (!_serversCopy.Contains(originalJenkinsServer))
            {
                return;
            }

            originalJenkinsServer.Name = newJenkinsServer.Name;
            originalJenkinsServer.Url = newJenkinsServer.Url;
            originalJenkinsServer.UserName = newJenkinsServer.UserName;
            originalJenkinsServer.ApiToken = newJenkinsServer.ApiToken;

            SaveJenkinsServers();
        }

        private static void SaveJenkinsServers()
        {
            var newJenkinsServerList = new JenkinsServerList();
            newJenkinsServerList.AddRange(_serversCopy);

            Settings.Default.JenkinsServers = newJenkinsServerList;
            Settings.Default.Save();
        }

        public static void UpdatePreferredView(string slnPath, string jenkinsViewName)
        {
            if(!ContainsSolutionPreference(slnPath))
            {
                return;
            }

            var jobLink = GetJobLink(slnPath);
            jobLink.JenkinsViewName = jenkinsViewName;
            Settings.Default.Save();
        }
    }
}
