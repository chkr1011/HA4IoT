using System;

namespace HA4IoT.Contracts.Components
{
    public class ComponentFeatureStateChangedEventArgs : EventArgs
    {
        public ComponentFeatureStateChangedEventArgs(IComponentFeatureState oldState, IComponentFeatureState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public IComponentFeatureState OldState { get; }

        public IComponentFeatureState NewState { get; }
    }
}
