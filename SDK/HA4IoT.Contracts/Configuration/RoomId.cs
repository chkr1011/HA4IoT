using System;
using Windows.Data.Json;
using HA4IoT.Networking;

namespace HA4IoT.Contracts.Configuration
{
    public class RoomId : IEquatable<RoomId>, IConvertibleToJsonValue
    {
        public RoomId(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Room ID is invalid.");

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

        public bool Equals(RoomId other)
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
