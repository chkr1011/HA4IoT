using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Sensors.Buttons
{
    public class ButtonSettingsWrapper : ComponentSettingsWrapper
    {
        private const string PressedLongDurationName = "PressedLongDuration";

        public ButtonSettingsWrapper(ISettingsContainer settings)
            : base(settings)
        {
            PressedLongDuration = TimeSpan.FromSeconds(1.5);
        }

        public TimeSpan PressedLongDuration
        {
            get { return Settings.GetTimeSpan(PressedLongDurationName); }
            set { Settings.SetValue(PressedLongDurationName, value); }
        }
    }
}
