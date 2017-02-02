using System;

namespace HA4IoT.Contracts.Components
{
    public class ComponentFeatureStateNotSupportedException : Exception
    {
        public ComponentFeatureStateNotSupportedException(IComponentFeatureState state)
            : base($"State '{state}' is not supported.")
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            State = state;
        }

        public IComponentFeatureState State { get; }
    }
}
