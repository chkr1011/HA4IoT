using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Backup;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Storage;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Settings
{
    [ApiServiceClass(typeof(ISettingsService))]
    public class SettingsService : ServiceBase, ISettingsService
    {
        private const string StorageFilename = "SettingsService.json";
        private const string BackupKeyName = "Settings";

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, JObject> _settings = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
        private readonly IStorageService _storageService;

        public SettingsService(IBackupService backupService, IStorageService storageService)
        {
            if (backupService == null) throw new ArgumentNullException(nameof(backupService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

            backupService.CreatingBackup += (s, e) => CreateBackup(e);
            backupService.RestoringBackup += (s, e) => RestoreBackup(e);
        }

        public void Initialize()
        {
            lock (_syncRoot)
            {
                TryLoadSettings();
            }
        }

        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        public void CreateSettingsMonitor<TSettings>(string uri, Action<SettingsChangedEventArgs<TSettings>> callback)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var initialSettings = GetSettings<TSettings>(uri);
            callback(new SettingsChangedEventArgs<TSettings>(default(TSettings), initialSettings));

            SettingsChanged += (s, e) =>
            {
                if (!e.Uri.Equals(uri))
                {
                    return;
                }

                TSettings oldSettings = default(TSettings);
                if (e.OldSettings != null)
                {
                    oldSettings = e.OldSettings.ToObject<TSettings>();
                }

                TSettings newSettings = default(TSettings);
                if (e.NewSettings != null)
                {
                    newSettings = e.NewSettings.ToObject<TSettings>();
                }

                callback(new SettingsChangedEventArgs<TSettings>(oldSettings, newSettings));
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

                SaveSettings();

                return settingsInstance;
            }
        }

        public JObject GetSettings(string uri)
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

        public void ImportSettings(string uri, object settings)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var rawSettings = settings as JObject ?? JObject.FromObject(settings);
            ImportRawSettings(uri, rawSettings);
        }

        [ApiMethod]
        public void Replace(IApiCall apiCall)
        {
            var request = apiCall.Parameter.ToObject<SettingsServiceApiRequest>();
            SetRawSettings(request.Uri, request.Settings);
        }

        [ApiMethod]
        public void Import(IApiCall apiCall)
        {
            if (apiCall.Parameter.Type == JTokenType.Object)
            {
                var request = apiCall.Parameter.ToObject<SettingsServiceApiRequest>();
                ImportRawSettings(request.Uri, request.Settings);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        [ApiMethod]
        public void ImportMultiple(IApiCall apiCall)
        {
            if (apiCall.Parameter.Type == JTokenType.Object)
            {
                var request = apiCall.Parameter.ToObject<Dictionary<string, JObject>>();
                foreach (var item in request)
                {
                    ImportSettings(item.Key, item.Value);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        [ApiMethod]
        public void GetSettings(IApiCall apiCall)
        {
            if (apiCall.Parameter.Type == JTokenType.Object)
            {
                var request = apiCall.Parameter.ToObject<SettingsServiceApiRequest>();
                apiCall.Result = GetSettings(request.Uri);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void CreateBackup(BackupEventArgs backupEventArgs)
        {
            lock (_syncRoot)
            {
                backupEventArgs.Backup[BackupKeyName] = JObject.FromObject(_settings);
            }
        }

        private void RestoreBackup(BackupEventArgs backupEventArgs)
        {
            if (backupEventArgs.Backup.Property(BackupKeyName) == null)
            {
                return;
            }

            var settings = backupEventArgs.Backup[BackupKeyName].Value<Dictionary<string, JObject>>();
            foreach (var setting in settings)
            {
                SetRawSettings(setting.Key, setting.Value);
            }
        }

        private void SetRawSettings(string uri, JObject settings)
        {
            lock (_syncRoot)
            {
                JObject oldSettings;
                _settings.TryGetValue(uri, out oldSettings);

                _settings[uri] = settings;
                SaveSettings();

                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(uri, oldSettings, settings));
            }
        }

        private void ImportRawSettings(string uri, JObject settings)
        {
            lock (_syncRoot)
            {
                JObject existingSettings;
                if (_settings.TryGetValue(uri, out existingSettings))
                {
                    var oldSettings = (JObject)existingSettings.DeepClone();
                    existingSettings.Merge(settings, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });

                    SaveSettings();
                    SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(uri, oldSettings, existingSettings));
                }
                else
                {
                    _settings[uri] = settings;
                    SaveSettings();
                    SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(uri, null, settings));
                }
            }
        }

        private void SaveSettings()
        {
            _storageService.Write(StorageFilename, _settings);
        }

        private void TryLoadSettings()
        {
            Dictionary<string, JObject> persistedSettings;
            if (_storageService.TryRead(StorageFilename, out persistedSettings))
            {
                foreach (var setting in persistedSettings)
                {
                    _settings[setting.Key] = setting.Value;
                }
            }
        }
    }
}
