using Devkoes.JenkinsManager.Model.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Devkoes.JenkinsManager.UI.ValidationRules
{
    public class JenkinsServerApiTokenRequired
    {
        public static IEnumerable<ValidationResult> Validate(JenkinsServer server)
        {
            if (!string.IsNullOrWhiteSpace(server.UserName) && string.IsNullOrWhiteSpace(server.ApiToken))
            {
                return new[] { new ValidationResult() { Message = "You need to specify an API key too." } };
            }

            return Enumerable.Empty<ValidationResult>();
        }
    }
}
