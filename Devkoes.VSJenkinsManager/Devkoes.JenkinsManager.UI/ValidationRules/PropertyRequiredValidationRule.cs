using Devkoes.JenkinsManager.Model.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Devkoes.JenkinsManager.UI.ValidationRules
{
    public static class PropertyRequiredValidationRule
    {
        public static IEnumerable<ValidationResult> Validate(string propertyName, string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                return new[] { new ValidationResult() {
                    Message = string.Concat(propertyName, " is a required field.")
                } };
            }

            return Enumerable.Empty<ValidationResult>();
        }
    }
}
