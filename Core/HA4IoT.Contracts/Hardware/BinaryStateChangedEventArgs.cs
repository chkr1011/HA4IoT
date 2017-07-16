using System;

namespace HA4IoT.Contracts.Hardware
{
    public sealed class BinaryStateChangedEventArgs : EventArgs
    {
        public BinaryStateChangedEventArgs(BinaryState oldState, BinaryState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public BinaryState OldState { get; }
        public BinaryState NewState { get; }
    }
}
