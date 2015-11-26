using System;

namespace HA4IoT.Contracts
{
    public class ActuatorId
    {
        public ActuatorId(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Actuator ID is invalid.");

            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
