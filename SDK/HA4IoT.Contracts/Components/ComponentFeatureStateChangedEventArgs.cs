using System;

namespace HA4IoT.Contracts.Components
{
    public class ComponentFeatureStateChangedEventArgs : EventArgs
    {
        public ComponentFeatureStateChangedEventArgs(IComponentFeatureStateCollection oldState, IComponentFeatureStateCollection newState)
        {
            if (oldState == null) throw new ArgumentNullException(nameof(oldState));
            if (newState == null) throw new ArgumentNullException(nameof(newState));

            OldState = oldState;
            NewState = newState;
        }

        public IComponentFeatureStateCollection OldState { get; }

        public IComponentFeatureStateCollection NewState { get; }
    }
}
