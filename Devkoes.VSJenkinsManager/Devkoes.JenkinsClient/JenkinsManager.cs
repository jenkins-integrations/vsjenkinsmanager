using Devkoes.JenkinsClient.Model;
using Devkoes.JenkinsClient.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Devkoes.JenkinsClient
{
    public class JenkinsManager
    {
        private const char URI_SEPERATOR = '|';

        /// <summary>
        /// Loads all job information
        /// </summary>
        /// <param name="jenkinsServerUrl">The url of the server, without any api paths (eg http://jenkins.cyanogenmod.com/)</param>
        /// <returns>The list of Jobs</returns>
        public async static Task<IEnumerable<Job>> GetJobs(string jenkinsServerUrl)
        {
            JenkinsOverview overview = null;

            try
            {
                WebClient wc = new WebClient();
                string jsonRawData = await wc.DownloadStringTaskAsync(Path.Combine(jenkinsServerUrl, "api/json?pretty=true"));
                overview = JsonConvert.DeserializeObject<JenkinsOverview>(jsonRawData);
            }
            catch (Exception)
            {
                // do something
            }

            // Fix when something went wrong, never return an empty list
            if (overview == null)
            {
                overview = new JenkinsOverview();
            }
            if (overview.Jobs == null)
            {
                overview.Jobs = new List<Job>();
            }

            return overview.Jobs;
        }

        public void AddServer(JenkinsServer server)
        {
            // TODO: figure out how to save custom objects in an array through settings
            Settings.Default.JenkinsServers.Add(string.Concat(server.Name, URI_SEPERATOR, server.Url));
            Settings.Default.Save();
        }

        public IEnumerable<JenkinsServer> GetServers()
        {
            var savedServers = Settings.Default.JenkinsServers;
            List<JenkinsServer> servers = new List<JenkinsServer>();
            foreach (var server in savedServers)
            {
                var parts = server.Split(URI_SEPERATOR);
                if (parts.Length == 2)
                {
                    servers.Add(new JenkinsServer() { Name = parts[0], Url = parts[1] });
                }
            }

            return servers;
        }
    }
}
