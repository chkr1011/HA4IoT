using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Settings
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public SettingsChangedEventArgs(string uri, JObject oldSettings, JObject newSettings)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            OldSettings = oldSettings;
            NewSettings = newSettings;
        }

        public string Uri { get; }

        public JObject OldSettings { get; }

        public JObject NewSettings { get; }
    }
}
