using Devkoes.JenkinsManager.Model.Schema;
using System.Collections.Generic;

namespace Devkoes.JenkinsManager.UI.ValidationRules
{
    public static class JenkinsServerResetApiToken
    {
        public static IEnumerable<ValidationResult> Validate(JenkinsServer server)
        {
            return new[] { new ResetValidationResult("ApiToken") };
        }
    }
}
