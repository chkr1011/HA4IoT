using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Areas
{
    public class AreaId : IdBase, IEquatable<AreaId>
    {
        public AreaId(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Area ID is invalid.");
        }

        public AreaId(Enum value) : this(value.ToString())
        {
        }

        public bool Equals(AreaId other)
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
