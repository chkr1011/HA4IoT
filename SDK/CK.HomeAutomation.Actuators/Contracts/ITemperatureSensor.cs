using System;
using CK.HomeAutomation.Hardware;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface ITemperatureSensor
    {
        event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        float Value { get; }
    }
}
