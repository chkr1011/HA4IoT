using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public interface IMotionDetectorSettings : IComponentSettings
    {
        TimeSpan AutoEnableAfter { get; set; }
    }
}
