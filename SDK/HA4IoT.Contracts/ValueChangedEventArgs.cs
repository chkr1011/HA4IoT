using System;

namespace HA4IoT.Contracts
{
    public class ValueChangedEventArgs<TValue> : EventArgs
    {
        public ValueChangedEventArgs(TValue oldValue, TValue newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public TValue OldValue { get; }

        public TValue NewValue { get; }
    }
}
