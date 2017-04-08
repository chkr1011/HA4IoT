using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public class ButtonSettings : ComponentSettings
    {
        public TimeSpan PressedLongDuration { get; set; } = TimeSpan.FromSeconds(1.5);
    }
}
