using System;

namespace HA4IoT.Contracts.Actuators
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(StateId oldState, StateId newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public StateId OldState { get; }

        public StateId NewState { get; }
    }
}
