using System;
using Windows.Data.Json;

namespace HA4IoT.Contracts.Services.Settings
{
    public interface ISettingsService : IService
    {
        void CreateSettingsMonitor<TSettings>(string uri, Action<TSettings> callback);

        TSettings GetSettings<TSettings>(string uri);

        JsonObject GetRawSettings(string uri);
    }
}
