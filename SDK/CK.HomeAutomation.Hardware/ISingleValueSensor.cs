using System;

namespace CK.HomeAutomation.Hardware
{
    public interface ISingleValueSensor
    {
        event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;
        float Read();
    }
}
