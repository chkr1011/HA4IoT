using System;

namespace CK.HomeAutomation.Core
{
    public class BinaryStateChangedEventArgs : EventArgs
    {
        public BinaryStateChangedEventArgs(BinaryState state)
        {
            State = state;
        }

        public BinaryState State { get; }
    }
}
