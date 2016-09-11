using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Services.Settings
{
    public interface ISettingsService : IService
    {
        void CreateSettingsMonitor<TSettings>(string uri, Action<TSettings> callback);

        TSettings GetSettings<TSettings>(string uri);

        JObject GetRawSettings(string uri);

        void SetSettings(string uri, object settings);

        void SetRawSettings(string uri, JObject settings);
    }
}
