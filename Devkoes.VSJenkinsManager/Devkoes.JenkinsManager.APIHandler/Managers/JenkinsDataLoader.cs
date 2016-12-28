﻿using Devkoes.JenkinsManager.Model.Schema;
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
        private const short MAX_JOB_BUILDS = 5;

        private const string VIEW_QUERY = "api/json?pretty=true&tree=views[name,url]";
        private const string JOBS_QUERY_BASE = "api/json?pretty=true&tree=jobs[name,url,inQueue,buildable,queueItem[why,id],property[parameterDefinitions[name,defaultParameterValue[value]]],builds[number,result,building,estimatedDuration,timestamp]";
        private const string JOBS_QUERY_WITH_RANGE = JOBS_QUERY_BASE + "{0,5}]";
        private const string JOBS_QUERY = JOBS_QUERY_BASE + "]";

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
                view.Url = FixPrimaryViewUrl(view);
            }

            return overview.Views;
        }

        private static string FixPrimaryViewUrl(JenkinsView view)
        {
            // Fix JSON problem which contains wrong url for primary view (is always the base url which contains
            // all builds, not just the ones for that view).
            if (!view.Url.ToUpperInvariant().Contains("/VIEW/"))
            {
                // We use Uri class to join url parts, we need a slash as last char. The
                // base url would be used if we didn't fix this, instead of the full url.
                if (view.Url.Last() != '/')
                {
                    view.Url += '/';
                }

                string viewUrlPart = string.Concat("view/", view.Name, "/");
                view.Url = new Uri(new Uri(view.Url), viewUrlPart).AbsoluteUri;
            }

            return view.Url;
        }

        public static async Task<IEnumerable<JenkinsJob>> GetJobsFromView(JenkinsServer server, JenkinsView view)
        {
            Uri viewUri = new Uri(view.Url);

            var rangeSpecifierSupported = JenkinsServerValidator.RangeSpecifierSupported(server.Url);
            var jobsQuery = rangeSpecifierSupported ? JOBS_QUERY_WITH_RANGE : JOBS_QUERY;

            Uri jobsInfoUri = new Uri(viewUri, jobsQuery);
            JenkinsView viewWithJobData = await GetFromJSONData<JenkinsView>(server, jobsInfoUri);

            var result = viewWithJobData == null ? Enumerable.Empty<JenkinsJob>() : viewWithJobData.Jobs;

            if (!rangeSpecifierSupported)
            {
                RestrictNumberOfJobBuilds(result);
            }

            return result;
        }

        private static void RestrictNumberOfJobBuilds(IEnumerable<JenkinsJob> jobs)
        {
            foreach (var job in jobs)
            {
                if (job.Builds != null && job.Builds.Count() > MAX_JOB_BUILDS)
                {
                    job.Builds = job.Builds.Take(5).ToArray();
                }
            }
        }

        public static WebClient CreateJenkinsWebClient(string jenkinsServerUrl)
        {
            JenkinsServer server = ApiHandlerSettingsManager.GetJenkinsServer(jenkinsServerUrl);

            return CreateJenkinsWebClient(server);
        }

        public static WebClient CreateJenkinsWebClient(JenkinsServer server)
        {
            WebClient client = new WebClient();
            client.UseDefaultCredentials = true;

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
                wc.Encoding = Encoding.UTF8;
                Task<string> jsonRawDataTask = wc.DownloadStringTaskAsync(jsonDataUri);

                if (await Task.WhenAny(jsonRawDataTask, Task.Delay(30000)) == jsonRawDataTask)
                {
                    deserialisedJsonObject = JsonConvert.DeserializeObject<T>(jsonRawDataTask.Result);
                }
            }

            return deserialisedJsonObject;
        }
    }
}
