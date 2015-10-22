using System;

namespace HA4IoT.Hardware
{
    public interface ISingleValueSensor
    {
        event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;
        float Read();
    }
}
