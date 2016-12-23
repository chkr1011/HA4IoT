using System;

namespace HA4IoT.Contracts.Components
{
    public class ComponentStateChangedEventArgs : EventArgs
    {
        public ComponentStateChangedEventArgs(ComponentState oldState, ComponentState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public ComponentState OldState { get; }

        public ComponentState NewState { get; }
    }
}
