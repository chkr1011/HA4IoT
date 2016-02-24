using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class ActuatorSettings : SettingsContainer, IActuatorSettings
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

        public ISetting<JsonObject> AppSettings { get; }

        public ISetting<bool> IsEnabled { get; }

        private static string GenerateFilename(ActuatorId actuatorId)
        {
            return StoragePath.WithFilename("Actuators", actuatorId.Value, "Configuration.json");
        }
    }
}
