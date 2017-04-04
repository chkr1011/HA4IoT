using System;

namespace HA4IoT.Contracts.Services.Settings
{
    public class SettingValueChangedEventArgs : EventArgs
    {
        public SettingValueChangedEventArgs(string settingName)
        {
            SettingName = settingName;
        }

        public string SettingName { get; set; }
    }
}
