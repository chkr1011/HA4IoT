using System;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutterSettings : IActuatorSettings
    {
        ISetting<int> MaxPosition { get; } 

        ISetting<TimeSpan> AutoOffTimeout { get; } 
    }
}
