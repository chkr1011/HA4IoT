using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public class StateId : IdBase, IEquatable<StateId>
    {
        public StateId(string value) 
            : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("State ID is invalid.");
        }

        public bool Equals(StateId other)
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
