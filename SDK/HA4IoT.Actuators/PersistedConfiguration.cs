using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Core;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class PersistedConfiguration
    {
        private readonly string _filename;
        private readonly JsonObject _configuration = new JsonObject();

        public PersistedConfiguration(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            
            _filename = filename;
            Load();
        }

        public JsonObject GetAsJson()
        {
            return _configuration;
        }

        public void SetValue(string name, string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_configuration)
            {
                _configuration.SetNamedValue(name, value.ToJsonValue());
            }
        }

        public string GetString(string name, string defaultValue = "")
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_configuration)
            {
                return _configuration.GetNamedString(name, defaultValue);
            }
        }

        public void Update(JsonObject request)
        {
            lock (_configuration)
            {
                foreach (var property in request.Keys)
                {
                    _configuration[property] = request[property];
                }

                Save();
            }
        }

        private void Load()
        {
            if (!File.Exists(_filename))
            {
                return;
            }

            try
            {
                string loadedConfigurationContent = File.ReadAllText(_filename);
                JsonObject loadedConfiguration = JsonObject.Parse(loadedConfigurationContent);

                _configuration.Clear();
                foreach (var key in loadedConfiguration.Keys)
                {
                    _configuration[key] = loadedConfiguration[key];
                }
            }
            catch (Exception exception)
            {
                ControllerBase.Log.Warning("Error while loading configuration from '{0}'. {1}", _filename, exception.Message);
            }
        }

        private void Save()
        {
            string directory = Path.GetDirectoryName(_filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_filename, _configuration.Stringify());
            ControllerBase.Log.Info("Persisted configuration at '{0}'.", _filename);
        }
    }
}
