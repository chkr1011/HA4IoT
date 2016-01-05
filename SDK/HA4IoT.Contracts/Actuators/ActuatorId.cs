using System;
using Windows.Data.Json;
using HA4IoT.Networking;

namespace HA4IoT.Contracts.Actuators
{
    public class ActuatorId : IEquatable<ActuatorId>, IConvertibleToJsonValue
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

        public IJsonValue ToJsonValue()
        {
            return JsonValue.CreateStringValue(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
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
