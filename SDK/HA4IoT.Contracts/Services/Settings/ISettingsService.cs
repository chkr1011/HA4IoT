using Windows.Data.Json;

namespace HA4IoT.Contracts.Services.Settings
{
    public interface ISettingsService : IService
    {
        TSettings GetSettings<TSettings>(string uri);

        JsonObject GetSettings(string uri);

        void SetSettings(string uri, object settings);

        void SetSettings(string uri, JsonObject settings);

        void ImportSettings(string uri, object settings);

        void ImportSettings(string uri, JsonObject settings);
    }
}
