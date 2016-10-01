using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.MotionDetectors
{
    public class MotionDetectorSettings : ComponentSettings, IMotionDetectorSettings
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
