using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public class MotionDetectorSettings : ComponentSettings
    {
        public TimeSpan AutoEnableAfter { get; set; } = TimeSpan.FromMinutes(60);
    }
}
