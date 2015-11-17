using Devkoes.JenkinsManager.APIHandler.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Devkoes.JenkinsManager.APIHandler.UnitTest.Managers
{
    [TestClass]
    public class JenkinsServerValidatorTest
    {
        private string _testUrl = "https://ab-jenkins1.companyname.com";

        [TestMethod]
        public void GetHeaders_HttpsUri_FilledHeaders()
        {
            var headers = JenkinsServerValidator.GetHeaders(_testUrl);

            Assert.IsNotNull(headers);
            Assert.AreNotEqual(0, headers.Count);
        }

        [TestMethod]
        public void GetHeaders_HttpsUri_XJenkinsHeaderAvailable()
        {
            var headers = JenkinsServerValidator.GetHeaders(_testUrl);

            Assert.IsNotNull(headers, "WebRequest failed");
            Assert.AreNotEqual(0, headers.Count, "No headers found");

            var hasXJenkinsHeader = headers.AllKeys.Contains(JenkinsServerValidator.JENKINS_VERSION_HEADER_KEY);

            Assert.IsTrue(hasXJenkinsHeader, "X-Jenkins header not found");
        }

        [TestMethod]
        public void GetJenkinsVersionFromHeaders_HttpsUri_ValidJenkinsVersion()
        {
            var headers = JenkinsServerValidator.GetHeaders(_testUrl);
            var version = JenkinsServerValidator.GetJenkinsVersionFromHeaders(headers);

            Assert.AreNotEqual(new Version(), version, "Version couldn't be parsed");
        }
    }
}
