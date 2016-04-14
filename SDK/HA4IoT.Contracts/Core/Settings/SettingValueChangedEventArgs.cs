using System;
using Windows.Data.Json;

namespace HA4IoT.Contracts.Core.Settings
{
    public class SettingValueChangedEventArgs : EventArgs
    {
        public SettingValueChangedEventArgs(string settingName, IJsonValue oldValue, IJsonValue newValue)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));
            if (oldValue == null) throw new ArgumentNullException(nameof(oldValue));
            if (newValue == null) throw new ArgumentNullException(nameof(newValue));

            SettingName = settingName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public string SettingName { get; }

        public IJsonValue OldValue { get; }

        public IJsonValue NewValue { get; }
    }
}
