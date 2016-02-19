using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class ActuatorSettings : SettingsContainer
    {
        public ActuatorSettings(ActuatorId actuatorId, ILogger logger) 
            : base(GenerateFilename(actuatorId), logger)
        {
            ActuatorId = actuatorId;

            IsEnabled = new Setting<bool>(true);
            AppSettings = new Setting<JsonObject>(new JsonObject());
        }

        [HideFromToJsonObject]
        public ActuatorId ActuatorId { get; private set; }

        public Setting<JsonObject> AppSettings { get; }

        public Setting<bool> IsEnabled { get; }

        private static string GenerateFilename(ActuatorId actuatorId)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actuators", actuatorId.Value, "Configuration.json");
        }
    }
}
