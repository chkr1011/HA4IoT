using System;

namespace HA4IoT.Contracts.Sensors
{
    public class NumericValueSensorEndpointValueChangedEventArgs : EventArgs
    {
        public NumericValueSensorEndpointValueChangedEventArgs(float newValue)
        {
            NewValue = newValue;
        }

        public float NewValue { get; }
    }
}
