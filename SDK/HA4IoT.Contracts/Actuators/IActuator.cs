using Windows.Data.Json;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuator
    {
        ActuatorId Id { get; }
        
        IActuatorSettings Settings { get; }

        JsonObject ExportConfigurationToJsonObject();

        JsonObject ExportStatusToJsonObject();

        void LoadSettings();
    }
}
