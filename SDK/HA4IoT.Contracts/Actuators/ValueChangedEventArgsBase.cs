using System;

namespace HA4IoT.Contracts.Actuators
{
    public class ValueChangedEventArgsBase<TValue> : EventArgs
    {
        public ValueChangedEventArgsBase(TValue oldValue, TValue newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public TValue OldValue { get; }

        public TValue NewValue { get; }
    }
}
