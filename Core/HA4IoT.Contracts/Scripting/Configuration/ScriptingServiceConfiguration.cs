using System.Collections.Generic;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Contracts.Scripting.Configuration
{
    public class ScriptingServiceConfiguration
    {
        public List<StartupScriptConfiguration> StartupScripts { get; set; } = new List<StartupScriptConfiguration>();
    }
}
