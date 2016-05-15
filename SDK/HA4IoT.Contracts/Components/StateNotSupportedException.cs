using System;

namespace HA4IoT.Contracts.Components
{
    public class StateNotSupportedException : Exception
    {
        public StateNotSupportedException(IComponentState state)
            : base($"State '{state}' is not supported.")
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            State = state;
        }

        public IComponentState State { get; }
    }
}
