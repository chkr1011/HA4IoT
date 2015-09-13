using System;

namespace CK.HomeAutomation.Hardware
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
