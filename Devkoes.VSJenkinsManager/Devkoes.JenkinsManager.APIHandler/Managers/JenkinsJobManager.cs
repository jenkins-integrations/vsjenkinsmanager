using Devkoes.JenkinsManager.Model.Schema;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    public static class JenkinsJobManager
    {
        public async static Task<string> GetLatestLog(string jobUrl, JenkinsServer jenkinsServer)
        {
            string logData = null;
            using (WebClient client = JenkinsDataLoader.CreateJenkinsWebClient(jenkinsServer))
            {
                var latestLogUri = CreateLatestLogUri(jobUrl);
                logData = await client.DownloadStringTaskAsync(latestLogUri);
            }

            string fileName = null;
            if (!string.IsNullOrWhiteSpace(logData))
            {
                fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
                File.WriteAllText(fileName, logData);
            }

            return fileName;
        }

        public async static Task ScheduleJob(string jobUrl, string jenkinsServerUrl)
        {
            using (WebClient client = JenkinsDataLoader.CreateJenkinsWebClient(jenkinsServerUrl))
            {
                Uri buildUri = CreateBuildUri(jobUrl);
                byte[] response = await client.UploadValuesTaskAsync(buildUri, new NameValueCollection()
                           {
                               { "delay", "0sec" }
                           }
                );
            }
        }

        private static Uri CreateBuildUri(string jobUrl)
        {
            var jobUri = new Uri(jobUrl);
            return new Uri(jobUri, "build");
        }

        private static Uri CreateLatestLogUri(string jobUrl)
        {
            var jobUri = new Uri(jobUrl);
            return new Uri(jobUri, "lastBuild/consoleText");
        }
    }
}
