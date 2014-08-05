using Devkoes.JenkinsManagerUI.Properties;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Devkoes.JenkinsManagerUI.Managers
{
    public static class SettingManager
    {
        private const string SOLUTIONJOBLINK_SEPERATOR = "THYSHALLNOTUSETHISSEPERATORINANYURL";
        private static Dictionary<string, string> _solutionPreferences;

        static SettingManager()
        {
            if (Settings.Default.SolutionJobLinks == null)
            {
                Settings.Default.SolutionJobLinks = new StringCollection();
            }

            _solutionPreferences = new Dictionary<string, string>();

            LoadSettings();
        }

        private static void LoadSettings()
        {
            _solutionPreferences.Clear();
            foreach (var item in Settings.Default.SolutionJobLinks)
            {
                var parts = item.Split(new[] { SOLUTIONJOBLINK_SEPERATOR }, System.StringSplitOptions.None);
                if (parts.Count() == 2)
                {
                    _solutionPreferences.Add(parts[0], parts[1]);
                }
            }
        }

        public static void SaveJobForSolution(string jobUri, string solutionPath)
        {
            if (ContainsSolutionPreference(solutionPath))
            {
                RemoveSolutionPreference(solutionPath);
            }

            _solutionPreferences.Add(solutionPath, jobUri);
            SaveSettings();
        }

        private static void RemoveSolutionPreference(string solutionPath)
        {
            _solutionPreferences.Remove(solutionPath);
            SaveSettings();
        }

        private static void SaveSettings()
        {
            var coll = new StringCollection();
            coll.AddRange(_solutionPreferences.Select(
                (kv) => string.Format("{0}{1}{2}", kv.Key, SOLUTIONJOBLINK_SEPERATOR, kv.Value)).ToArray());

            Settings.Default.SolutionJobLinks = coll;
            Settings.Default.Save();
        }

        public static bool ContainsSolutionPreference(string solutionPath)
        {
            return _solutionPreferences.ContainsKey(solutionPath);
        }   

        public static string GetJobUri(string solutionPath)
        {
            if (!string.IsNullOrEmpty(solutionPath) && _solutionPreferences.ContainsKey(solutionPath))
            {
                return _solutionPreferences[solutionPath];
            }

            return null;
        }
    }
}
