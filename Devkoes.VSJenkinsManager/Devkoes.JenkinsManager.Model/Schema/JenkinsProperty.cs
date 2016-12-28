using System.Collections.Generic;
using Newtonsoft.Json;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class JenkinsProperty
    {
        [JsonProperty("ParameterDefinitions")]
        public IEnumerable<JenkinsParameterDefinition> ParameterDefinitions { get; set; }

    }
}