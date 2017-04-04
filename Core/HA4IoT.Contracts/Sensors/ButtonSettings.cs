using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public class ButtonSettings : ComponentSettings
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
