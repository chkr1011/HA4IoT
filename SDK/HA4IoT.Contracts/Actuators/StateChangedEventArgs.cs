using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(StatefulComponentState oldState, StatefulComponentState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public StatefulComponentState OldState { get; }

        public StatefulComponentState NewState { get; }
    }
}
