using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface INumericValueSensorAdapter
    {
        event EventHandler<NumericValueSensorEndpointValueChangedEventArgs> ValueChanged;
    }
}
