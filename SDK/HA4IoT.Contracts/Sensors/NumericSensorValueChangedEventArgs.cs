using System;

namespace HA4IoT.Contracts.Sensors
{
    public class NumericSensorValueChangedEventArgs : EventArgs
    {
        public NumericSensorValueChangedEventArgs(float oldValue, float newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public float OldValue { get; }
        public float NewValue { get; }
    }
}
