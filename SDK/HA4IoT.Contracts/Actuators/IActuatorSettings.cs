using Windows.Data.Json;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuatorSettings
    {
        ActuatorId ActuatorId { get; }

        ISetting<bool> IsEnabled { get; }

        ISetting<JsonObject> AppSettings { get; }

        JsonObject ExportToJsonObject();

        void ImportFromJsonObject(JsonObject requestBody);

        void Load();
    }
}
