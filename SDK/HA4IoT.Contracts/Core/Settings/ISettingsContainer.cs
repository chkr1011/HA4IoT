using System;
using Windows.Data.Json;

namespace HA4IoT.Contracts.Core.Settings
{
    public interface ISettingsContainer
    {
        event EventHandler<SettingValueChangedEventArgs> ValueChanged;

        JsonObject Export();

        void Import(JsonObject requestBody);
        
        void Load();

        void Save();

        bool GetBoolean(string name);

        TimeSpan GetTimeSpan(string name);

        int GetInteger(string name);

        float GetFloat(string name);

        string GetString(string name);

        void SetValue(string name, float value);

        void SetValue(string name, bool value);

        void SetValue(string name, string value);

        void SetValue(string name, TimeSpan value);

        void SetValue(string name, int number);

        void SetValue(string name, JsonObject value);
    }
}
