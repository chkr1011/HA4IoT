using System;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Tests.Mockups
{
    public class TestNumericValueSensorEndpoint : INumericValueSensorEndpoint
    {
        public event EventHandler<NumericValueSensorEndpointValueChangedEventArgs> ValueChanged;

        public void UpdateValue(float value)
        {
            ValueChanged?.Invoke(this, new NumericValueSensorEndpointValueChangedEventArgs(value));
        }
    }
}
