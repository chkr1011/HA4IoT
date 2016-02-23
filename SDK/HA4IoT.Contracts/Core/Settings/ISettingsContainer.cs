using Windows.Data.Json;

namespace HA4IoT.Contracts.Core.Settings
{
    public interface ISettingsContainer
    {
        JsonObject ExportToJsonObject();

        void ImportFromJsonObject(JsonObject requestBody);
    }
}
