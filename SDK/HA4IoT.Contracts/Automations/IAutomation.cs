using Windows.Data.Json;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomation
    {
        AutomationId Id { get; }

        JsonObject ExportConfigurationAsJsonValue();

        JsonObject ExportStatusToJsonObject();

        void LoadSettings();
    }
}
