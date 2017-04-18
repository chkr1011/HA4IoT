using System;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDeviceStateChangedEventArgs : EventArgs
    {
        public CCToolsDeviceStateChangedEventArgs(byte[] oldState, byte[] newState)
        {
            OldState = oldState ?? throw new ArgumentNullException(nameof(oldState));
            NewState = newState ?? throw new ArgumentNullException(nameof(newState));
        }

        public byte[] OldState { get; }

        public byte[] NewState { get; }
    }
}
