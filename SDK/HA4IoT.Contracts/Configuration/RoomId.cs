using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Configuration
{
    public class RoomId : IdBase, IEquatable<RoomId>
    {
        public RoomId(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Room ID is invalid.");
        }

        public static RoomId From(Enum value)
        {
            return new RoomId(value.ToString());
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
