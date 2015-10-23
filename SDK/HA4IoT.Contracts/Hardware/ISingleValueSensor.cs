using System;

namespace HA4IoT.Contracts.Hardware
{
    public interface ISingleValueSensor
    {
        event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;
        float Read();
    }
}
