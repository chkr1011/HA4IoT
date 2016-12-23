using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Hardware
{
    public class DeviceId : IdBase, IEquatable<DeviceId>
    {
        public DeviceId(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Device ID is invalid.");
        }

        public bool Equals(DeviceId other)
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
