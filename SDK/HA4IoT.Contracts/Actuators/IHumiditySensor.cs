using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IHumiditySensor
    {
        event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        float Value { get; }
    }
}
