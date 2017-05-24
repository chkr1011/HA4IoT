using System;

namespace HA4IoT.Contracts.Components.Adapters
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
