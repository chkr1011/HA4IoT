using System;

namespace HA4IoT.Contracts.Adapters
{
    public class SensorAdapterValueChangedEventArgs : EventArgs
    {
        public SensorAdapterValueChangedEventArgs(float newValue)
        {
            NewValue = newValue;
        }

        public float NewValue { get; }
    }
}
