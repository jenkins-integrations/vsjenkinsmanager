using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Devkoes.JenkinsManager.APIHandler.Managers
{
    /// <summary>
    /// 1.568 - introduced range specifier
    /// 1.367 - introduced tree specifier
    /// </summary>
    public static class JenkinsServerValidator
    {
        /// <summary>
        /// Cache with versions of valid Jenkins servers
        /// </summary>
        private static readonly Dictionary<string, Version> _validJenkinsVersionCache = new Dictionary<string, Version>();

        private const string JENKINS_VERSION_HEADER_KEY = "X-Jenkins";

        public static readonly Version MINIMUM_VERSION = new Version("1.367");
        public static readonly Version RANGE_SPECIFIER_VERSION = new Version("1.568");

        public static Version GetJenkinsVersion(string jenkinsServerUrl)
        {
            if (_validJenkinsVersionCache.ContainsKey(jenkinsServerUrl))
            {
                return _validJenkinsVersionCache[jenkinsServerUrl];
            }

            WebHeaderCollection headers = GetHeaders(jenkinsServerUrl);

            if (headers == null)
            {
                // request failed, don't cache and return empty version
                return new Version();
            }

            Version jenkinsVersion = GetJenkinsVersionFromHeaders(headers);

            _validJenkinsVersionCache[jenkinsServerUrl] = jenkinsVersion;

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

        public static bool RangeSpecifierSupported(string jenkinsServerUrl)
        {
            var version = GetJenkinsVersion(jenkinsServerUrl);

            return version >= RANGE_SPECIFIER_VERSION;
        }

        private static Version GetJenkinsVersionFromHeaders(WebHeaderCollection headers)
        {
            Version jenkinsVersion = null;
            if (headers.AllKeys.Contains(JENKINS_VERSION_HEADER_KEY))
            {
                var versionString = headers[JENKINS_VERSION_HEADER_KEY];
                Version.TryParse(versionString, out jenkinsVersion);
            }

            return jenkinsVersion ?? new Version();
        }

        private static WebHeaderCollection GetHeaders(string url)
        {
            WebHeaderCollection headersFromRequestUrl = null;
            try
            {
                var req = WebRequest.Create(url);
                req.Timeout = 1000;
                req.Method = "HEAD";
                var response = req.GetResponse();

                headersFromRequestUrl = response.Headers;
            }
            catch (WebException ex)
            {
                // A WebException could occur when authorization is needed (403 forbidden)
                // We can still return the headers if a response is available
                if (ex.Response != null)
                {
                    headersFromRequestUrl = ex.Response.Headers;
                }
            }
            catch { }

            return headersFromRequestUrl;
        }
    }
}
