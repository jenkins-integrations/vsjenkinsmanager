using Devkoes.JenkinsManager.Model.Schema;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

        public async static Task BuildJob(string jobUrl, string jenkinsServerUrl)
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

        public async static Task ScheduleJob(string jobUrl, string jenkinsServerUrl)
        {
            using (WebClient client = JenkinsDataLoader.CreateJenkinsWebClient(jenkinsServerUrl))
            {
                Uri buildUri = CreateBuildUri(jobUrl);
                byte[] response = await client.UploadValuesTaskAsync(buildUri, new NameValueCollection());
            }
        }

        public static async Task BuildJobWithDefaultParameters(JenkinsJob job, string jenkinsServerUrl)
        {
            using (var client = JenkinsDataLoader.CreateJenkinsWebClient(jenkinsServerUrl))
            {
                Uri buildUri = CreateBuildWithParametersUri(job.Url);
                var parametersNameValueCollection = new NameValueCollection();
                job.Property.Aggregate(parametersNameValueCollection, (collection, property) =>
                {
                    if (property.ParameterDefinitions != null && property.ParameterDefinitions.Any())
                    {
                        property.ParameterDefinitions.ToList().ForEach(pd => collection.Add(pd.Name, pd.DefaultParameterValue.Value));
                    }
                    return collection;
                });
                parametersNameValueCollection.Add("delay", "0sec");
                byte[] response = await client.UploadValuesTaskAsync(buildUri, parametersNameValueCollection);
            }
        }

        public async static Task DequeueJob(JenkinsQueueItem queueItem, string jenkinsServerUrl)
        {
            if (queueItem == null)
            {
                throw new ArgumentNullException("queueItem");
            }

            Uri dequeueUri = CreateDeqeueuBuildUri(jenkinsServerUrl, queueItem);
            using (WebClient client = JenkinsDataLoader.CreateJenkinsWebClient(jenkinsServerUrl))
            {
                byte[] response = await client.UploadValuesTaskAsync(dequeueUri, new NameValueCollection());
            }
        }

        public async static Task CancelBuild(JenkinsJob job, string jenkinsServerUrl)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            Uri cancelBuildUri = CreateCancelBuildUri(job);

            if (cancelBuildUri != null)
            {
                using (WebClient client = JenkinsDataLoader.CreateJenkinsWebClient(jenkinsServerUrl))
                {
                    byte[] response = await client.UploadValuesTaskAsync(cancelBuildUri, new NameValueCollection());
                }
            }
        }

        private static Uri CreateBuildUri(string jobUrl)
        {
            var jobUri = new Uri(jobUrl);
            return new Uri(jobUri, "build");
        }

        private static Uri CreateDeqeueuBuildUri(string jenkinsServerUrl, JenkinsQueueItem queueItem)
        {
            var serverUri = new Uri(jenkinsServerUrl);
            return new Uri(serverUri, "queue/cancelItem?id=" + queueItem.ID);
        }

        private static Uri CreateCancelBuildUri(JenkinsJob job)
        {
            var serverUri = new Uri(job.Url);
            if (job.LatestBuild != null && job.LatestBuild.IsBuilding)
            {
                return new Uri(serverUri, job.LatestBuild.Number + "/stop");
            }

            return null;
        }

        private static Uri CreateLatestLogUri(string jobUrl)
        {
            var jobUri = new Uri(jobUrl);
            return new Uri(jobUri, "lastBuild/consoleText");
        }

        private static Uri CreateBuildWithParametersUri(string jobUrl)
        {
            var joubUri = new Uri(jobUrl);
            return new Uri(joubUri, "buildWithParameters");
        }
    }
}
