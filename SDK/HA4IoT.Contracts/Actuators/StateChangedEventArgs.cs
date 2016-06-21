using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(NamedComponentState oldState, NamedComponentState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public NamedComponentState OldState { get; }

        public NamedComponentState NewState { get; }
    }
}
