using Windows.Data.Json;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomation
    {
        AutomationId Id { get; }

        ISettingsContainer Settings { get; }

        IAutomationSettingsWrapper GeneralSettingsWrapper { get; }

        JsonObject ExportConfigurationAsJsonValue();

        JsonObject ExportStatusToJsonObject();
    }
}
