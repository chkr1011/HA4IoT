using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Sensors
{
    public class SensorId : IdBase, IEquatable<SensorId>
    {
        public SensorId(string value) 
            : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Sensor ID is invalid.");
        }

        public bool Equals(SensorId other)
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
