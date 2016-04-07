using System;

namespace HA4IoT.Contracts.Sensors
{
    public class SensorValueChangedEventArgs : EventArgs
    {
        public SensorValueChangedEventArgs(ISensorValue oldValue, ISensorValue newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public ISensorValue OldValue { get; }

        public ISensorValue NewValue { get; }
    }
}
