using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Components
{
    public class ComponentId : IdBase, IEquatable<ComponentId>
    {
        public ComponentId(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Component ID is invalid.");
        }

        public bool Equals(ComponentId other)
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
