using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public class ComponentStateChangedEventArgs : EventArgs
    {
        public ComponentStateChangedEventArgs(IComponentState oldState, IComponentState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public IComponentState OldState { get; }

        public IComponentState NewState { get; }
    }
}
