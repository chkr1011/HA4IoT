using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public class MotionDetectorSettings : ComponentSettings
    {
        private TimeSpan _autoEnableAfter = TimeSpan.FromMinutes(60);

        public TimeSpan AutoEnableAfter
        {
            get { return _autoEnableAfter; }
            set
            {
                _autoEnableAfter = value;
                OnValueChanged(nameof(AutoEnableAfter));
            }
        }
    }
}
