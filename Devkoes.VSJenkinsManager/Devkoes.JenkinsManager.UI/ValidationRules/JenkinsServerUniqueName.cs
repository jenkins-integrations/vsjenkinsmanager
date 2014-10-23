using Devkoes.JenkinsManager.APIHandler.Managers;
using Devkoes.JenkinsManager.Model.Schema;
using Devkoes.JenkinsManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devkoes.JenkinsManager.UI.ValidationRules
{
    public static class JenkinsServerUniqueName
    {
        public static IEnumerable<ValidationResult> Validate(JenkinsServer editedServer)
        {
            var servers = ApiHandlerSettingsManager.GetServers();
            var equalServers = servers.Where((s) => string.Equals(editedServer.Name, s.Name, StringComparison.InvariantCultureIgnoreCase));
            if (equalServers.Any((s) => s != ViewModelController.BasicUserOptionsContentViewModel.SelectedJenkinsServer))
            {
                return new[] { new ValidationResult() { Message = "Server name must be unique." } };
            }

            return Enumerable.Empty<ValidationResult>();
        }
    }
}
