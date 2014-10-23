using Devkoes.JenkinsManager.Model.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    public static class JenkinsDataLoader
    {
        private const string VIEW_QUERY = "api/json?pretty=true&tree=views[name,url]";
        private const string JOBS_QUERY_WITH_RANGE = "api/json?pretty=true&tree=jobs[name,url,inQueue,buildable,builds[result,building,estimatedDuration,timestamp]{0,5},queueItem[why]]";
        private const string JOBS_QUERY = "api/json?pretty=true&tree=jobs[name,url,inQueue,buildable,builds[result,building,estimatedDuration,timestamp],queueItem[why]]";

        public static async Task<IEnumerable<JenkinsView>> GetViews(JenkinsServer server)
        {
            Uri serverUri = new Uri(server.Url);
            Uri viewInfoUri = new Uri(serverUri, VIEW_QUERY);
            JenkinsOverview overview = await GetFromJSONData<JenkinsOverview>(server, viewInfoUri);

            if (overview == null)
            {
                overview = new JenkinsOverview();
            }

            foreach (var view in overview.Views)
            {
                view.Url = FixViewUrl(view);
            }

            return overview.Views;
        }

        private static string FixViewUrl(JenkinsView view)
        {
            // Fix JSON problem which contains wrong url for primary view (is always the base url which contains
            // all builds, not just the ones for that view).
            if (!view.Url.ToUpperInvariant().Contains("/VIEW/"))
            {
                view.Url = new Uri(new Uri(new Uri(view.Url), "/view/"), view.Name + "/").AbsoluteUri;
            }

            return view.Url;
        }

        public static async Task<IEnumerable<JenkinsJob>> GetJobsFromView(JenkinsServer server, JenkinsView view)
        {
            Uri viewUri = new Uri(view.Url);

            var jobsQuery = JenkinsServerValidator.RangeSpecifierSupported(server.Url) ? JOBS_QUERY_WITH_RANGE : JOBS_QUERY;

            Uri jobsInfoUri = new Uri(viewUri, jobsQuery);
            JenkinsView viewWithJobData = await GetFromJSONData<JenkinsView>(server, jobsInfoUri);

            return viewWithJobData == null ? Enumerable.Empty<JenkinsJob>() : viewWithJobData.Jobs;
        }

        public static WebClient CreateJenkinsWebClient(string jenkinsServerUrl)
        {
            JenkinsServer server = ApiHandlerSettingsManager.GetJenkinsServer(jenkinsServerUrl);

            return CreateJenkinsWebClient(server);
        }

        public static WebClient CreateJenkinsWebClient(JenkinsServer server)
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

        public async static Task<T> GetFromJSONData<T>(string jenkinsServerUrl, Uri jsonDataUri) where T : class
        {
            return await GetFromJSONData<T>(ApiHandlerSettingsManager.GetJenkinsServer(jenkinsServerUrl), jsonDataUri);
        }

        public async static Task<T> GetFromJSONData<T>(JenkinsServer server, Uri jsonDataUri) where T : class
        {
            T deserialisedJsonObject = null;

            using (WebClient wc = CreateJenkinsWebClient(server))
            {
                Task<string> jsonRawDataTask = wc.DownloadStringTaskAsync(jsonDataUri);

                if (await Task.WhenAny(jsonRawDataTask, Task.Delay(5000)) == jsonRawDataTask)
                {
                    deserialisedJsonObject = JsonConvert.DeserializeObject<T>(jsonRawDataTask.Result);
                }
            }

            return deserialisedJsonObject;
        }
    }
}
