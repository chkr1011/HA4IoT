using System;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Contracts.Components
{
    public class ComponentSettings : IComponentSettings
    {
        private bool _isEnabled = true;
        private string _caption;

        public event EventHandler<SettingValueChangedEventArgs> ValueChanged;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnValueChanged(nameof(IsEnabled));
            }
        }

        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnValueChanged(nameof(Caption));
            }
        }

        protected virtual void OnValueChanged(string settingName)
        {
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(settingName));
        }
    }
}
