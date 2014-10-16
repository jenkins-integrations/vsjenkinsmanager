
using System.Windows.Controls;
namespace Devkoes.JenkinsManager.UI.ValidationRules
{
    public class JenkinsServerVersionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return new ValidationResult(false, "Always invalid");
        }
    }
}
