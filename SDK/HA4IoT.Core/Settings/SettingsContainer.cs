using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Core.Settings
{
    public abstract class SettingsContainer
    {
        private readonly ILogger _logger;
        private readonly string _filename;

        protected SettingsContainer(string filename, ILogger logger)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            _filename = filename;
        }

        public void Load()
        {
            if (!File.Exists(_filename))
            {
                return;
            }

            try
            {
                string fileContent = File.ReadAllText(_filename);
                JsonObject jsonObject = JsonObject.Parse(fileContent);

                ImportFromJsonObject(jsonObject);
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Error while loading settings from '{0}'.", _filename);
            }
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(_filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_filename, ExportToJsonObject().Stringify());
            _logger.Verbose("Saved settings at '{0}'.", _filename);
        }

        public JsonObject ExportToJsonObject()
        {
            return this.ToJsonObject();
        }

        public void ImportFromJsonObject(JsonObject value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            value.DeserializeTo(this);
            Save();
        }

        public void ImportFromJsonObjectWithoutSaving(JsonObject value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            value.DeserializeTo(this);
        }
    }
}
