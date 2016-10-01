using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Buttons
{
    public class ButtonSettings : ComponentSettings, IButtonSettings
    {
        private TimeSpan _pressedLongDuration = TimeSpan.FromSeconds(1.5);

        public TimeSpan PressedLongDuration
        {
            get { return _pressedLongDuration; }
            set
            {
                _pressedLongDuration = value;
                OnValueChanged(nameof(PressedLongDuration));
            }
        }
    }
}
