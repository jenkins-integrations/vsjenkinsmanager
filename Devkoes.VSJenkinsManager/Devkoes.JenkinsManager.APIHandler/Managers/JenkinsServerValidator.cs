using System;
using System.Net;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    /// <summary>
    /// 1.568 - introduced range specifier
    /// 1.367 - introduced tree specifier
    /// </summary>
    public static class JenkinsServerValidator
    {
        public static readonly Version MINIMUM_VERSION;

        static JenkinsServerValidator()
        {
            MINIMUM_VERSION = new Version("1.367");
        }

        public static Version GetJenkinsVersion(string url)
        {
            Version jenkinsVersion = null;
            try
            {
                var req = WebRequest.Create(url);
                req.Method = "HEAD";
                var response = req.GetResponse();

                var versionString = response.Headers["X-Jenkins"];

                Version.TryParse(versionString, out jenkinsVersion);
            }
            catch { }

            jenkinsVersion = jenkinsVersion ?? new Version();

            return jenkinsVersion;
        }

        public static bool ValidJenkinsServer(string url)
        {
            var version = GetJenkinsVersion(url);

            return version >= MINIMUM_VERSION;
        }
    }
}
