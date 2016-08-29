using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.RollerShutters
{
    public class RollerShutterSettings : ComponentSettings
    {
        public int MaxPosition { get; set; } = 20000;

        public TimeSpan AutoOffTimeout { get; set; } = TimeSpan.FromSeconds(22);
    }
}
