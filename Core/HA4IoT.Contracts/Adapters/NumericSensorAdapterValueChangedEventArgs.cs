using System;

namespace HA4IoT.Contracts.Adapters
{
    public class NumericSensorAdapterValueChangedEventArgs : EventArgs
    {
        public NumericSensorAdapterValueChangedEventArgs(float? value)
        {
            Value = value;
        }

        public float? Value { get; }
    }
}
