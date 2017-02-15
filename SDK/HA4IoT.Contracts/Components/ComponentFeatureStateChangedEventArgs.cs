using System;

namespace HA4IoT.Contracts.Components
{
    public class ComponentFeatureStateChangedEventArgs : EventArgs
    {
        public ComponentFeatureStateChangedEventArgs(ComponentFeatureStateCollection oldState, ComponentFeatureStateCollection newState)
        {
            if (oldState == null) throw new ArgumentNullException(nameof(oldState));
            if (newState == null) throw new ArgumentNullException(nameof(newState));

            OldState = oldState;
            NewState = newState;
        }

        public ComponentFeatureStateCollection OldState { get; }

        public ComponentFeatureStateCollection NewState { get; }
    }
}
