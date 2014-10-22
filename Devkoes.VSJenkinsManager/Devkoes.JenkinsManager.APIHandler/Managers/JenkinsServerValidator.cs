using Devkoes.JenkinsManager.Model.Schema;
using System;
using System.Collections.Generic;
using System.Net;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    /// <summary>
    /// 1.568 - introduced range specifier
    /// 1.367 - introduced tree specifier
    /// </summary>
    public static class JenkinsServerValidator
    {
        private static readonly Dictionary<string, Version> _jenkinsServerVersions = new Dictionary<string, Version>();

        public static readonly Version MINIMUM_VERSION = new Version("1.367");
        public static readonly Version RANGE_SPECIFIER_VERSION = new Version("1.568");

        public static Version GetJenkinsVersion(string jenkinsServerUrl)
        {
            if (_jenkinsServerVersions.ContainsKey(jenkinsServerUrl))
            {
                return _jenkinsServerVersions[jenkinsServerUrl];
            }

            Version jenkinsVersion = null;
            try
            {
                var req = WebRequest.Create(jenkinsServerUrl);
                req.Timeout = 1000;
                req.Method = "HEAD";
                var response = req.GetResponse();

                var versionString = response.Headers["X-Jenkins"];

                Version.TryParse(versionString, out jenkinsVersion);
            }
            catch { }

            jenkinsVersion = jenkinsVersion ?? new Version();

            _jenkinsServerVersions[jenkinsServerUrl] = jenkinsVersion;

            return jenkinsVersion;
        }

        public static bool IsJenkinsServer(string jenkinsServerUrl)
        {
            var version = GetJenkinsVersion(jenkinsServerUrl);

            return version != new Version();
        }

        public static bool IsMinimumRequiredVersion(string jenkinsServerUrl)
        {
            var version = GetJenkinsVersion(jenkinsServerUrl);

            return version >= MINIMUM_VERSION;
        }

        public static bool RangeSpecifierSupported(JenkinsServer jenkinsServer)
        {
            return jenkinsServer.Version >= RANGE_SPECIFIER_VERSION;
        }
    }
}
