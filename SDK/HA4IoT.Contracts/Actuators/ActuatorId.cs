using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public class ActuatorId : IdBase, IEquatable<ActuatorId>
    {
        public ActuatorId(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Actuator ID is invalid.");
        }

        public static ActuatorId From(Enum value)
        {
            return new ActuatorId(value.ToString());
        }

        public bool Equals(ActuatorId other)
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
