using System;

namespace HA4IoT.Settings
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public SettingsChangedEventArgs(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Uri = uri;
        }

        public string Uri { get; }
    }
}
