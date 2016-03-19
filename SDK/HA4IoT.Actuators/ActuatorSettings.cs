using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class ActuatorSettings : SettingsContainer, IActuatorSettings
    {
        public ActuatorSettings(ActuatorId actuatorId) 
            : base(GenerateFilename(actuatorId))
        {
            ActuatorId = actuatorId;

            IsEnabled = new Setting<bool>(true);
            AppSettings = new Setting<JsonObject>(new JsonObject());
        }

        [HideFromToJsonObject]
        public ActuatorId ActuatorId { get; }

        public ISetting<bool> IsEnabled { get; }

        public ISetting<JsonObject> AppSettings { get; }

        private static string GenerateFilename(ActuatorId actuatorId)
        {
            string oldFilename = StoragePath.WithFilename("Actuators", actuatorId.Value, "Configuration.json");
            string newFilename = StoragePath.WithFilename("Actuators", actuatorId.Value, "Settings.json");

            if (File.Exists(oldFilename))
            {
                if (File.Exists(newFilename))
                {
                    File.Delete(oldFilename);
                }

                File.Move(oldFilename, newFilename);
            }

            return newFilename;
        }
    }
}
