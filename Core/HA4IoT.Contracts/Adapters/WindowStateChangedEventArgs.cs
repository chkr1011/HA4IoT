using System;

namespace HA4IoT.Contracts.Adapters
{
    public class WindowStateChangedEventArgs : EventArgs
    {
        public WindowStateChangedEventArgs(ReedSwitchState openReedSwitchState, ReedSwitchState? tildReedSwitchState)
        {
            OpenReedSwitchState = openReedSwitchState;
            TildReedSwitchState = tildReedSwitchState;
        }

        public ReedSwitchState OpenReedSwitchState { get; }

        public ReedSwitchState? TildReedSwitchState { get; }
    }
}
