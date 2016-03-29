using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public class StateMachineStateId : IdBase, IEquatable<StateMachineStateId>
    {
        public StateMachineStateId(string value) 
            : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("State Machine ID is invalid.");
        }

        public bool Equals(StateMachineStateId other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return Value.Equals(other.Value);
        }
    }
}
