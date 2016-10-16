using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Backup;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Storage;
using HA4IoT.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Resources
{
    [ApiServiceClass(typeof(IResourceService))]
    public class ResourceService : ServiceBase, IResourceService
    {
        private const string StorageFilename = "ResourceService.json";
        private const string BackupKeyName = "Resources";

        private readonly object _syncRoot = new object();
        private readonly List<Resource> _resources = new List<Resource>();
        private readonly IStorageService _storageService;
        private readonly ISettingsService _settingsService;

        private ControllerSettings _controllerSettings;
        
        public ResourceService(IBackupService backupService, IStorageService storageService, ISettingsService settingsService)
        {
            if (backupService == null) throw new ArgumentNullException(nameof(backupService));
            if (storageService == null) throw new ArgumentNullException(nameof(storageService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _storageService = storageService;
            _settingsService = settingsService;

            backupService.CreatingBackup += (s, e) => CreateBackup(e);
            backupService.RestoringBackup += (s, e) => RestoreBackup(e);
        }

        public void Initialize()
        {
            _controllerSettings = _settingsService.GetSettings<ControllerSettings>();

            lock (_syncRoot)
            {
                TryLoadResources();
            }
        }

        public void RegisterText(Enum id, string value)
        {
            var uri = GenerateUri(id);

            lock (_syncRoot)
            {
                var resource = _resources.FirstOrDefault(r => r.Uri.Equals(uri));
                if (resource != null)
                {
                    return;
                }

                resource = new Resource(uri, value);
                _resources.Add(resource);
                
                SaveResources();
            }
        }

        public string GetText(Enum id)
        {
            var uri = GenerateUri(id);

            lock (_syncRoot)
            {
                var resource = _resources.FirstOrDefault(r => r.Uri.Equals(uri));
                if (resource == null)
                {
                    return $"#Resource '{uri}' not found.";
                }

                var resourceValue = resource.Values.FirstOrDefault(rv => rv.LanguageCode.Equals(_controllerSettings.Language));
                if (resourceValue == null)
                {
                    return resource.DefaultValue;
                }

                return resourceValue.Value;
            }
        }

        public string GetText(Enum id, params object[] formatParameterObjects)
        {
            if (formatParameterObjects == null) throw new ArgumentNullException(nameof(formatParameterObjects));

            var text = GetText(id);
            foreach (var formatParameter in formatParameterObjects)
            {
                foreach (var property in formatParameter.GetType().GetProperties())
                {
                    text = ReplaceFormatParameter(text, property.Name, property.GetValue(formatParameter));
                }
            }

            return text;
        }

        public string GetText(Enum id, IDictionary<string, object> formatParameters)
        {
            if (formatParameters == null) throw new ArgumentNullException(nameof(formatParameters));

            var text = GetText(id);
            foreach (var formatParameter in formatParameters)
            {
                text = ReplaceFormatParameter(text, formatParameter.Key, formatParameter.Value);
            }

            return text;
        }

        [ApiMethod]
        public void SetTexts(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                var request = apiContext.Request.ToObject<SetTextsRequest>();
                if (request?.Resources == null || !request.Resources.Any())
                {
                    apiContext.ResultCode = ApiResultCode.InvalidBody;
                    return;
                }

                foreach (var updatedResource in request.Resources)
                {
                    var existingResource = _resources.FirstOrDefault(r => r.Uri.Equals(updatedResource.Uri));
                    if (existingResource != null)
                    {
                        _resources.Remove(existingResource);
                    }

                    _resources.Add(updatedResource);
                }
                
                SaveResources();
            }
        }

        [ApiMethod]
        public void GetTexts(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                var request = apiContext.Request.ToObject<GetTextsRequest>();

                var matchingResources = _resources;
                if (!string.IsNullOrEmpty(request.Category))
                {
                    matchingResources = _resources.Where(r => r.Uri.StartsWith(request.Category + ".")).ToList();
                }

                apiContext.Response["Resources"] = JToken.FromObject(matchingResources);
            }
        }

        private void TryLoadResources()
        {
            List<Resource> persistedResources;
            if (_storageService.TryRead(StorageFilename, out persistedResources))
            {
                _resources.AddRange(persistedResources);
            }
        }

        private void SaveResources()
        {
            _storageService.Write(StorageFilename, _resources);
        }

        private string GenerateUri(Enum id)
        {
            return $"{id.GetType().Name}.{id}";
        }

        private string ReplaceFormatParameter(string text, string name, object value)
        {
            return text.Replace("{" + name + "}", Convert.ToString(value));
        }

        private void CreateBackup(BackupEventArgs backupEventArgs)
        {
            lock (_syncRoot)
            {
                backupEventArgs.Backup[BackupKeyName] = JToken.FromObject(_resources);
            }
        }

        private void RestoreBackup(BackupEventArgs backupEventArgs)
        {
            if (backupEventArgs.Backup.Property(BackupKeyName) == null)
            {
                return;
            }

            var resources = backupEventArgs.Backup[BackupKeyName].Value<List<Resource>>();

            lock (_syncRoot)
            {
                _resources.Clear();
                _resources.AddRange(resources);
            }
        }
    }
}
