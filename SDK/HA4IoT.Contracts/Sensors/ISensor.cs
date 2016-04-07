using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public interface ISensor : IComponent
    {
        event EventHandler<SensorValueChangedEventArgs> CurrentValueChanged;

        ISensorValue GetCurrentValue();
    }
}
