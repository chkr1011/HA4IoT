using System;
using System.Collections.Generic;
using System.IO;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Settings
{
    [ApiServiceClass(typeof(ISettingsService))]
    public class SettingsService : ServiceBase, ISettingsService
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, JObject> _settings = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
        
        public void Initialize()
        {
            lock (_syncRoot)
            {
                TryLoadSettings();
            }
        }

        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        public void CreateSettingsMonitor<TSettings>(string uri, Action<TSettings> callback)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var initialSettings = GetSettings<TSettings>(uri);
            callback(initialSettings);

            SettingsChanged += (s, e) =>
            {
                if (e.Uri.Equals(uri))
                {
                    var updateSettings = GetSettings<TSettings>(uri);
                    callback(updateSettings);
                }
            };
        }

        public TSettings GetSettings<TSettings>(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            lock (_syncRoot)
            {
                JObject settings;
                if (_settings.TryGetValue(uri, out settings))
                {
                    return settings.ToObject<TSettings>();
                }

                var settingsInstance = Activator.CreateInstance<TSettings>();
                _settings[uri] = JObject.FromObject(settingsInstance);

                Save();

                return settingsInstance;
            }
        }

        public JObject GetRawSettings(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            lock (_syncRoot)
            {
                JObject settings;
                if (!_settings.TryGetValue(uri, out settings))
                {
                    settings = new JObject();
                }

                return settings;
            }
        }

        private void SetSettings(string uri, JObject settings)
        {
            lock (_syncRoot)
            {
                _settings[uri] = settings;

                Save();

                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(uri));
            }
        }

        public void ImportSettings(string uri, JObject settings)
        {
            lock (_syncRoot)
            {
                JObject existingSettings;
                if (_settings.TryGetValue(uri, out existingSettings))
                {
                    existingSettings.Merge(settings, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
                }
                else
                {
                    _settings[uri] = settings;
                }

                Save();

                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(uri));
            }
        }

        [ApiMethod(ApiCallType.Command)]
        public void Replace(IApiContext apiContext)
        {
            var uri = (string)apiContext.Request["Uri"];
            var settings = (JObject)apiContext.Request["Settings"];

            SetSettings(uri, settings);
        }

        [ApiMethod(ApiCallType.Command)]
        public void Import(IApiContext apiContext)
        {
            var uri = (string)apiContext.Request["Uri"];
            var settings = (JObject)apiContext.Request["Settings"];

            ImportSettings(uri, settings);
        }

        [ApiMethod(ApiCallType.Request)]
        public void Settings(IApiContext apiContext)
        {
            var uri = (string)apiContext.Request["Uri"];

            apiContext.Response = GetRawSettings(uri);
        }

        [ApiMethod(ApiCallType.Request)]
        public void Backup(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                apiContext.Response = JObject.FromObject(_settings);
            }
        }

        private void Save()
        {
            var filename = StoragePath.WithFilename("SettingsService.json");
            var content = JsonConvert.SerializeObject(_settings, Formatting.Indented);
            
            File.WriteAllText(filename, content);
        }

        private void TryLoadSettings()
        {
            try
            {
                var filename = StoragePath.WithFilename("SettingsService.json");
                if (!File.Exists(filename))
                {
                    return;
                }

                var fileContent = File.ReadAllText(filename);
                if (string.IsNullOrEmpty(fileContent))
                {
                    return;
                }

                var settings = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(fileContent);
                foreach (var setting in settings)
                {
                    _settings[setting.Key] = setting.Value;
                }
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to load settings.");
            }
        }
    }
}
