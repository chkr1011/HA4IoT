using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Networking.Json;

namespace HA4IoT.Settings
{
    [ApiServiceClass(typeof(ISettingsService))]
    public class SettingsService : ServiceBase, ISettingsService
    {
        private readonly object _syncRoot = new object();
        private JsonObject _settings = new JsonObject();
        
        public void Initialize()
        {
            lock (_syncRoot)
            {
                TryLoadSettings();
            }
        }

        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        public TSettings GetSettings<TSettings>(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            lock (_syncRoot)
            {
                var settings = Activator.CreateInstance<TSettings>();

                IJsonValue value;
                if (!_settings.TryGetValue(uri, out value))
                {
                    SetSettings(uri, settings);
                }
                else
                {
                    if (value.ValueType != JsonValueType.Object)
                    {
                        throw new InvalidOperationException("Settings must be a JSON object.");
                    }

                    value.GetObject().DeserializeTo(settings);
                }

                SettingsChanged += (s, e) =>
                {
                    if (e.Uri.Equals(uri))
                    {
                        GetSettings(uri).DeserializeTo(settings);
                    }
                };
                
                return settings;
            }
            
        }

        public JsonObject GetSettings(string uri)
        {
            lock (_syncRoot)
            {
                IJsonValue value;
                if (!_settings.TryGetValue(uri, out value))
                {
                    value = new JsonObject();
                }

                if (value.ValueType != JsonValueType.Object)
                {
                    throw new InvalidOperationException("Settings must be a JSON object.");
                }

                return value.GetObject();
            }
        }

        public void SetSettings(string uri, object settings)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var rawSettings = settings.ToJsonObject();
            SetSettings(uri, rawSettings);
        }

        public void SetSettings(string uri, JsonObject settings)
        {
            lock (_syncRoot)
            {
                _settings[uri] = settings;
                Save();

                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(uri));
            }
        }

        public void ImportSettings(string uri, object settings)
        {
            ImportSettings(uri, settings.ToJsonObject());
        }

        public void ImportSettings(string uri, JsonObject settings)
        {
            lock (_syncRoot)
            {
                var existingSettings = GetSettings(uri);
                existingSettings.Import(settings);

                _settings[uri] = existingSettings;
                Save();
            }
        }

        [ApiMethod(ApiCallType.Command)]
        public void Replace(IApiContext apiContext)
        {
            var uri = apiContext.Request.GetNamedString("Uri");
            var settings = apiContext.Request.GetNamedObject("Settings");

            SetSettings(uri, settings);
        }

        [ApiMethod(ApiCallType.Command)]
        public void Import(IApiContext apiContext)
        {
            var uri = apiContext.Request.GetNamedString("Uri");
            var settings = apiContext.Request.GetNamedObject("Settings");

            ImportSettings(uri, settings);
        }

        [ApiMethod(ApiCallType.Request)]
        public void Settings(IApiContext apiContext)
        {
            var uri = apiContext.Request.GetNamedString("Uri");

            apiContext.Response = GetSettings(uri);
        }

        [ApiMethod(ApiCallType.Request)]
        public void Backup(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                apiContext.Response = JsonObject.Parse(_settings.ToString());
            }
        }

        private void Save()
        {
            var filename = StoragePath.WithFilename("SettingsService.json");
            var buffer = _settings.ToString();

            File.WriteAllText(filename, buffer);
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

                JsonObject jsonObject;
                if (!JsonObject.TryParse(fileContent, out jsonObject))
                {
                    return;
                }

                _settings = jsonObject;
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to load settings.");
            }
        }
    }
}
