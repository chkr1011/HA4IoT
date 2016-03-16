using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Core.Settings;

namespace HA4IoT.Actuators
{
    public class ButtonSettings : ActuatorSettings
    {
        public ButtonSettings(ActuatorId actuatorId) 
            : base(actuatorId)
        {
            PressedLongDuration = new Setting<TimeSpan>(TimeSpan.FromSeconds(1.5));
        }

        public Setting<TimeSpan> PressedLongDuration { get; } 
    }
}
