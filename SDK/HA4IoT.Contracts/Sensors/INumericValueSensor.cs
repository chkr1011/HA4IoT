using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface INumericValueSensor : ISensor
    {
        event EventHandler<NumericSensorValueChangedEventArgs> CurrentNumericValueChanged; 

        float GetCurrentNumericValue();
    }
}
