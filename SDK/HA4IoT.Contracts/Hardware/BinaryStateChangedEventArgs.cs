using System;

namespace HA4IoT.Contracts.Hardware
{
    public class BinaryStateChangedEventArgs : EventArgs
    {
        public BinaryStateChangedEventArgs(BinaryState newState)
        {
            NewState = newState;
        }

        public BinaryState NewState { get; }
    }
}
