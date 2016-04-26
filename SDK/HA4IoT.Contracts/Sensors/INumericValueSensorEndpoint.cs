using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface INumericValueSensorEndpoint
    {
        event EventHandler<NumericValueSensorEndpointValueChangedEventArgs> ValueChanged;
    }
}
