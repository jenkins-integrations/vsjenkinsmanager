using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    public static class JenkinsJobManager
    {
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
    }
}
