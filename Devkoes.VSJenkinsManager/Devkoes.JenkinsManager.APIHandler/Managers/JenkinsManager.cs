using Devkoes.JenkinsManager.APIHandler.Model;
using Devkoes.JenkinsManager.APIHandler.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    public class JenkinsManager
    {
        private static string JENKINS_BUILD_PREFIX_TEXT = "_anime";
        private static Dictionary<string, string> _colorScheme;

        static JenkinsManager()
        {
            if (Settings.Default.JenkinsServers == null)
            {
                Settings.Default.JenkinsServers = new JenkinsServerList();
            }

            _colorScheme = new Dictionary<string, string>() {
                { "red"+JENKINS_BUILD_PREFIX_TEXT, "Yellow" },
                { "red", "Firebrick" },
                { "blue"+JENKINS_BUILD_PREFIX_TEXT, "Yellow" },
                { "blue", "ForestGreen" },
                { "yellow", "Yellow"},
                { "yellow"+JENKINS_BUILD_PREFIX_TEXT, "Yellow"},
                { "disabled", "Gray" }
            };
        }

        /// <summary>
        /// Loads all job information
        /// </summary>
        /// <param name="jenkinsServerUrl">The url of the server, without any api paths (eg http://jenkins.cyanogenmod.com/)</param>
        /// <returns>The list of Jobs</returns>
        public async static Task<JenkinsOverview> GetJenkinsOverview(string jenkinsServerUrl)
        {
            JenkinsServer server = SettingManager.GetJenkinsServer(jenkinsServerUrl);

            Uri overviewUri = CreateOverviewUri(jenkinsServerUrl);
            JenkinsOverview overview = await GetFromJSONData<JenkinsOverview>(server, overviewUri);

            Uri queueUri = CreateQueueUri(jenkinsServerUrl);
            JenkinsQueue queue = await GetFromJSONData<JenkinsQueue>(server, queueUri);

            queue.Items = queue.Items ?? new List<ScheduledJob>();

            List<Job> allJobs = await GetJobsFromViews(server, overview.Views);

            overview.Jobs = ParseJobs(allJobs, queue);

            return overview;
        }

        private static async Task<List<Job>> GetJobsFromViews(JenkinsServer server, IEnumerable<View> allViews)
        {
            object allJobLock = new object();
            var allJobs = new List<Job>();
            foreach (var view in allViews.AsParallel())
            {
                view.Url = FixViewUrl(view);

                JenkinsView viewData = await GetJenkinsView(server, view.Url);

                if (viewData == null)
                    continue;

                foreach (var job in viewData.Jobs)
                {
                    // A job can exist in multiple views. We want only one Job object per job, use the Job
                    // object from the allJobs list if it's already there.
                    lock (allJobLock)
                    {
                        // TODO: use Equals override to check for equality?
                        var allJobsJob = allJobs.FirstOrDefault((j) => string.Equals(j.Url, job.Url));
                        if (allJobsJob != null)
                        {
                            view.Jobs.Add(allJobsJob);
                        }
                        else
                        {
                            view.Jobs.Add(job);
                            allJobs.Add(job);
                        }
                    }
                }
            }

            return allJobs;
        }

        private static string FixViewUrl(View view)
        {
            // Fix JSON problem which contains wrong url for primary view (is always the base url which contains
            // all builds, not just the ones for that view).
            if (!view.Url.ToUpperInvariant().Contains("/VIEW/"))
            {
                view.Url = string.Format("{0}/view/{1}/", view.Url, view.Name);
            }

            return view.Url;
        }

        private async static Task<JenkinsView> GetJenkinsView(JenkinsServer server, string viewUrl)
        {
            Uri viewUri = CreateViewUri(viewUrl);
            JenkinsView jView = await GetFromJSONData<JenkinsView>(server, viewUri);

            return jView;
        }

        private static IEnumerable<Job> ParseJobs(IEnumerable<Job> jobs, JenkinsQueue queue)
        {
            foreach (var job in jobs)
            {
                if (string.IsNullOrEmpty(job.Name) || string.IsNullOrEmpty(job.Color))
                    continue;

                if (job.Color.Contains(JENKINS_BUILD_PREFIX_TEXT))
                {
                    job.Building = true;
                }

                if (_colorScheme.ContainsKey(job.Color))
                {
                    job.Color = _colorScheme[job.Color];
                }

                var queueItem = queue.Items.FirstOrDefault((i) => string.Equals(i.Task.Url, job.Url));

                if (queueItem != null)
                {
                    job.Queued = true;
                    job.QueuedWhy = queueItem.Why;
                }
            }

            return jobs;
        }

        public async static Task ScheduleJob(string jobUrl, string jenkinsServerUrl)
        {
            using (WebClient client = CreateJenkinsWebClient(jenkinsServerUrl))
            {
                Uri buildUri = CreateBuildUri(jobUrl);
                byte[] response = await client.UploadValuesTaskAsync(buildUri, new NameValueCollection()
                   {
                       { "delay", "0sec" }
                   }
                );
            }
        }

        private static WebClient CreateJenkinsWebClient(string jenkinsServerUrl)
        {
            JenkinsServer server = SettingManager.GetJenkinsServer(jenkinsServerUrl);

            return CreateJenkinsWebClient(server);
        }

        private static WebClient CreateJenkinsWebClient(JenkinsServer server)
        {
            WebClient client = new WebClient();
            if (server != null && !string.IsNullOrWhiteSpace(server.UserName))
            {
                // WebClient.Credentials can not be used, because those credentials will only be send to the server
                // when the server responds with a challenge from the server. Jenkins won't send this challenge as documented
                // on the wiki: https://wiki.jenkins-ci.org/display/JENKINS/Authenticating+scripted+clients

                // We should use the "old fashion" way of setting the header manually
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", server.UserName, server.ApiToken)));
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            }

            return client;
        }

        private async static Task<T> GetFromJSONData<T>(string jenkinsServerUrl, Uri jsonDataUri) where T : class
        {
            return await GetFromJSONData<T>(SettingManager.GetJenkinsServer(jenkinsServerUrl), jsonDataUri);
        }

        private async static Task<T> GetFromJSONData<T>(JenkinsServer server, Uri jsonDataUri) where T : class
        {
            T deserialisedJsonObject = null;

            using (WebClient wc = CreateJenkinsWebClient(server))
            {
                Task<string> jsonRawDataTask = wc.DownloadStringTaskAsync(jsonDataUri);

                if (await Task.WhenAny(jsonRawDataTask, Task.Delay(3000)) == jsonRawDataTask)
                {
                    deserialisedJsonObject = JsonConvert.DeserializeObject<T>(jsonRawDataTask.Result);
                }
            }

            return deserialisedJsonObject;
        }

        private static Uri CreateBuildUri(string jobUrl)
        {
            var jobUri = new Uri(jobUrl);
            return new Uri(jobUri, "build");
        }

        private static Uri CreateOverviewUri(string jenkinsServerUrl)
        {
            Uri baseUri = new Uri(jenkinsServerUrl);
            return new Uri(baseUri, "api/json?pretty=true&tree=views[name,url]");
        }

        private static Uri CreateQueueUri(string jenkinsServerUrl)
        {
            Uri baseUri = new Uri(jenkinsServerUrl);
            return new Uri(baseUri, "queue/api/json?pretty=true&tree=items[why,task[name,url,color]]");
        }

        private static Uri CreateViewUri(string jenkinsViewUrl)
        {
            Uri viewUri = new Uri(jenkinsViewUrl);
            return new Uri(viewUri, "api/json?pretty=true");
        }
    }
}
