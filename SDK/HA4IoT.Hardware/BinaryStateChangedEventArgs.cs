using System;

namespace HA4IoT.Hardware
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
