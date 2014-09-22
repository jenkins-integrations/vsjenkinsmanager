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
            JenkinsOverview overview = null;
            JenkinsQueue queue = null;

            Uri baseUri = new Uri(jenkinsServerUrl);
            Uri overviewUri = new Uri(baseUri, "api/json?pretty=true&tree=views[name,url]");

            string jsonOverviewData = await GetWebData(overviewUri);

            if (!string.IsNullOrWhiteSpace(jsonOverviewData))
            {
                overview = JsonConvert.DeserializeObject<JenkinsOverview>(jsonOverviewData) ?? new JenkinsOverview();

                var queueUri = new Uri(baseUri, "queue/api/json?pretty=true&tree=items[why,task[name,url,color]]");
                string jsonQueueData = await GetWebData(queueUri);

                queue = JsonConvert.DeserializeObject<JenkinsQueue>(jsonQueueData) ?? new JenkinsQueue();

                queue.Items = queue.Items ?? new List<ScheduledJob>();

                object allJobLock = new object();
                var allJobs = new List<Job>();
                foreach (var view in overview.Views.AsParallel())
                {
                    FixViewUrl(view);

                    JenkinsView viewData = await GetJenkinsView(view.Url);

                    foreach (var job in viewData.Jobs)
                    {
                        lock (allJobLock)
                        {
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

                overview.Jobs = ParseJobs(allJobs, queue);

                return overview;
            }

            return new JenkinsOverview();
        }

        private static void FixViewUrl(View view)
        {
            // Fix JSON problem which contains wrong url for primary view (is always the base url which contains
            // all builds, not just the ones for that view).
            if (!view.Url.Contains("/view/"))
            {
                view.Url = string.Format("{0}/view/{1}/", view.Url, view.Name);
            }
        }

        private async static Task<string> GetWebData(Uri url)
        {
            WebClient wc = new WebClient();

            Task<string> jsonRawDataTask = wc.DownloadStringTaskAsync(url);
            if (await Task.WhenAny(jsonRawDataTask, Task.Delay(3000)) == jsonRawDataTask)
            {
                return jsonRawDataTask.Result;
            }

            return null;
        }

        private async static Task<JenkinsView> GetJenkinsView(string viewUrl)
        {
            JenkinsView view = null;
            WebClient wc = new WebClient();
            Uri baseUri = new Uri(viewUrl);

            Task<string> jsonRawDataTask = wc.DownloadStringTaskAsync(new Uri(baseUri, "api/json?pretty=true"));

            if (await Task.WhenAny(jsonRawDataTask, Task.Delay(3000)) == jsonRawDataTask)
            {
                view = JsonConvert.DeserializeObject<JenkinsView>(jsonRawDataTask.Result);
            }

            return view ?? new JenkinsView();
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

        public async static Task ScheduleJob(string jobUrl, string jenkinsServerUri)
        {
            JenkinsServer server = SettingManager.GetJenkinsServer(jenkinsServerUri);

            if (server == null)
            {
                return;
            }

            using (WebClient client = new WebClient())
            {
                if (!string.IsNullOrWhiteSpace(server.UserName))
                {
                    // WebClient.Credentials can not be used, because those credentials will only be send to the server
                    // when the server responds with a challenge from the server. Jenkins won't send this challenge as documented
                    // on the wiki: https://wiki.jenkins-ci.org/display/JENKINS/Authenticating+scripted+clients

                    // We should use the "old fashion" way of setting the header manually
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", server.UserName, server.ApiToken)));
                    client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                }

                var jobUri = new Uri(jobUrl);
                var requestUri = new Uri(jobUri, "build");
                byte[] response = await client.UploadValuesTaskAsync(requestUri, new NameValueCollection()
                   {
                       { "delay", "0sec" }
                   }
                );
            }
        }
    }
}
