using System;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Contracts.Components
{
    public class ComponentSettings : IComponentSettings
    {
        private bool _isEnabled = true;

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

        protected virtual void OnValueChanged(string settingName)
        {
            ValueChanged?.Invoke(this, new SettingValueChangedEventArgs(settingName));
        }
    }
}
