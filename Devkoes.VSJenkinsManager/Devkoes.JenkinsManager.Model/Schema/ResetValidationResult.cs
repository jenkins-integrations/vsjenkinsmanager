
namespace Devkoes.JenkinsManager.Model.Schema
{
    public class ResetValidationResult : ValidationResult
    {
        public string PropertyName { get; private set; }

        public ResetValidationResult(string propName)
            : base()
        {
            PropertyName = propName;
        }
    }
}
