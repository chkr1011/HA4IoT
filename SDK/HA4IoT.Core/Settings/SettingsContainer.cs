using System;
using System.Globalization;
using System.IO;
using System.Text;
using Windows.Data.Json;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Core.Settings
{
    public class SettingsContainer : ISettingsContainer
    {
        private JsonObject _settingsJson = new JsonObject();
        private readonly string _filename;

        public SettingsContainer(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            _filename = filename;
        }

        public event EventHandler<SettingValueChangedEventArgs> ValueChanged;

        public void Load()
        {
            if (!File.Exists(_filename))
            {
                return;
            }

            string fileContent = string.Empty;
            try
            {
                fileContent = File.ReadAllText(_filename, Encoding.UTF8);
                _settingsJson = JsonObject.Parse(fileContent);
            }
            catch (Exception exception)
            {
                Log.Warning(exception, $"Error while loading settings from '{_filename}' ({fileContent}).");
                //TODO: File.Delete(_filename);
            }
        }

        public string GetString(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _settingsJson.GetNamedString(name);
        }

        public float GetFloat(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return (float)_settingsJson.GetNamedNumber(name);
        }

        public bool GetBoolean(string name)
        {
            return _settingsJson.GetNamedBoolean(name);
        }

        public TimeSpan GetTimeSpan(string name)
        {
            return TimeSpan.Parse(_settingsJson.GetNamedString(name), DateTimeFormatInfo.InvariantInfo);
        }

        public int GetInteger(string name)
        {
            return (int)_settingsJson.GetNamedNumber(name);
        }

        public void SetValue(string name, string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _settingsJson.SetNamedString(name, value);
        }

        public void SetValue(string name, float value)
        {
            _settingsJson.SetNamedNumber(name, value);
        }

        public void SetValue(string name, bool value)
        {
            _settingsJson.SetNamedBoolean(name, value);
        }

        public void SetValue(string name, TimeSpan value)
        {
            _settingsJson.SetNamedString(name, value.ToString("c"));
        }

        public void SetValue(string name, int value)
        {
            _settingsJson.SetNamedNumber(name, value);
        }

        public void SetValue(string name, JsonObject value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _settingsJson.SetNamedObject(name, value);
        }

        public JsonObject Export()
        {
            return _settingsJson;
        }

        public void Import(JsonObject source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            foreach (var key in source.Keys)
            {
                IJsonValue value = source.GetNamedValue(key);
                IJsonValue existingValue = JsonValue.CreateNullValue();

                if (_settingsJson.ContainsKey(key))
                {
                    existingValue = _settingsJson.GetNamedValue(key);

                    if (value.ValueType != existingValue.ValueType)
                    {
                        Log.Warning($"Settings import skipped setting '{key}' due to different type.");
                        continue;
                    }

                    if (existingValue.Equals(value))
                    {
                        continue;
                    }
                }

                _settingsJson.SetNamedValue(key, value);
                ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(key, existingValue, value));
            }
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(_filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            //TODO: File.WriteAllText(_filename, _settingsJson.Stringify(), Encoding.UTF8);
            Log.Verbose($"Saved settings at '{_filename}'.");
        }
    }
}
