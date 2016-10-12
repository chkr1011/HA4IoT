using System;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentSettings
    {
        event EventHandler<SettingValueChangedEventArgs> ValueChanged;

        bool IsEnabled { get; set; }

        string Caption { get; set; }
    }
}
