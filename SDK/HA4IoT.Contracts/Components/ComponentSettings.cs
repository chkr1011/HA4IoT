using System;
using System.Runtime.CompilerServices;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Contracts.Components
{
    public class ComponentSettings : IComponentSettings
    {
        private bool _isEnabled = true;
        private string _caption;
        private string _keywords;

        public event EventHandler<SettingValueChangedEventArgs> ValueChanged;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnValueChanged(); }
        }

        public string Caption
        {
            get { return _caption; }
            set { _caption = value; OnValueChanged(); }
        }

        public string Keywords
        {
            get { return _keywords; }
            set { _keywords = value; OnValueChanged(); }
        }

        protected virtual void OnValueChanged([CallerMemberName] string settingName = null)
        {
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(settingName));
        }
    }
}
