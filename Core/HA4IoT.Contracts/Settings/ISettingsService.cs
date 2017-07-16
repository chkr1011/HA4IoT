using System;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Settings
{
    public interface ISettingsService : IService
    {
        void CreateSettingsMonitor<TSettings>(string uri, Action<SettingsChangedEventArgs<TSettings>> callback);

        TSettings GetSettings<TSettings>(string uri);

        JObject GetSettings(string uri);

        void ImportSettings(string uri, object settings);
    }
}
