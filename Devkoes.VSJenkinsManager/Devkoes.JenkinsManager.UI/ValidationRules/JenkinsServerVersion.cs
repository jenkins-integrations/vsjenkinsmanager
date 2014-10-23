using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Devkoes.JenkinsManager.UI.ValidationRules
{
    public static class JenkinsServerVersion
    {
        public static Task<IEnumerable<ValidationResult>> Validate(JenkinsServer server)
        {
            return Task.Factory.StartNew(() =>
                {
                    if (string.IsNullOrWhiteSpace(server.Url))
                    {
                        return Enumerable.Empty<ValidationResult>();
                    }

                    if (!JenkinsServerValidator.IsJenkinsServer(server.Url))
                    {
                        return new[] { new ValidationResult() { Message = "Url is not a valid Jenkins server." } };
                    }

                    var version = JenkinsServerValidator.GetJenkinsVersion(server.Url);

                    if (!JenkinsServerValidator.IsMinimumRequiredVersion(server.Url))
                    {
                        return new[] { 
                            new ValidationResult() { 
                                Message = string.Format(
                                    "You need at least Jenkins version {0} (found {1}).", 
                                    JenkinsServerValidator.MINIMUM_VERSION, version)
                            }
                        };
                    }

                    if (!JenkinsServerValidator.RangeSpecifierSupported(server.Url))
                    {
                        return new[] { 
                            new ValidationResult() { 
                                ValidationResultType = ValidationResultType.Warning,
                                Message = string.Format(
                                    "Data loading can be slow on this Jenkins version, please upgrade to version {0} to fix this.", 
                                    JenkinsServerValidator.RANGE_SPECIFIER_VERSION)
                            }
                        };
                    }

                    return Enumerable.Empty<ValidationResult>();
                });
        }
    }
}
