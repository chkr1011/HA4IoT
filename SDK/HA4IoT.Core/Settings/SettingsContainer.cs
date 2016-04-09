using System;
using System.Globalization;
using System.IO;
using System.Text;
using Windows.Data.Json;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Core.Settings
{
    public class SettingsContainer : ISettingsContainer
    {
        private readonly JsonObject _settingsJson = new JsonObject();
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
                var persistedSettings = JsonObject.Parse(fileContent);

                foreach (var settingName in persistedSettings.Keys)
                {
                    _settingsJson.SetNamedValue(settingName, persistedSettings[settingName]);
                }

                MigrateOldSettings();
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

            UpdateValue(name, JsonValue.CreateStringValue(value));
        }

        public void SetValue(string name, float value)
        {
            UpdateValue(name, JsonValue.CreateNumberValue(value));
        }

        public void SetValue(string name, bool value)
        {
            UpdateValue(name, JsonValue.CreateBooleanValue(value));
        }

        public void SetValue(string name, TimeSpan value)
        {
            UpdateValue(name, JsonValue.CreateStringValue(value.ToString("c")));
        }

        public void SetValue(string name, int value)
        {
            UpdateValue(name, JsonValue.CreateNumberValue(value));
        }

        public void SetValue(string name, JsonObject value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            UpdateValue(name, value);
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
                UpdateValue(key, value);
            }
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(_filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_filename, _settingsJson.Stringify(), Encoding.UTF8);
            Log.Verbose($"Saved settings at '{_filename}'.");
        }

        private void MigrateOldSettings()
        {
            if (_settingsJson.ContainsKey("AppSettings"))
            {
                _settingsJson["appSettings"] = _settingsJson["AppSettings"];
                _settingsJson.Remove("AppSettings");
                Save();
            }
        }

        private void UpdateValue(string name, IJsonValue value)
        {
            IJsonValue existingValue = JsonValue.CreateNullValue();

            if (_settingsJson.ContainsKey(name))
            {
                existingValue = _settingsJson.GetNamedValue(name);

                if (value.ValueType != existingValue.ValueType)
                {
                    Log.Warning($"Skipped update of setting '{name}' due to different value types.");
                    return;
                }

                if (existingValue.Stringify().Equals(value.Stringify()))
                {
                    return;
                }
            }

            _settingsJson.SetNamedValue(name, value);
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(name, existingValue, value));
        }
    }
}
