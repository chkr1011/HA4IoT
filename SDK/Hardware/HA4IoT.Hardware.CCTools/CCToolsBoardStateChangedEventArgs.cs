using System;

namespace HA4IoT.Hardware.CCTools
{
    public class IOBoardStateChangedEventArgs : EventArgs
    {
        public IOBoardStateChangedEventArgs(byte[] oldState, byte[] newState)
        {
            if (oldState == null) throw new ArgumentNullException(nameof(oldState));
            if (newState == null) throw new ArgumentNullException(nameof(newState));

            OldState = oldState;
            NewState = newState;
        }

        public byte[] OldState { get; }

        public byte[] NewState { get; }
    }
}
