using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButtonSettings : IComponentSettings
    {
        TimeSpan PressedLongDuration { get; set; }
    }
}
