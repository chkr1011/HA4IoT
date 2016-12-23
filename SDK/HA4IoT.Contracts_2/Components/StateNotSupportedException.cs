using System;

namespace HA4IoT.Contracts.Components
{
    public class StateNotSupportedException : Exception
    {
        public StateNotSupportedException(ComponentState state)
            : base($"State '{state}' is not supported.")
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            State = state;
        }

        public ComponentState State { get; }
    }
}
