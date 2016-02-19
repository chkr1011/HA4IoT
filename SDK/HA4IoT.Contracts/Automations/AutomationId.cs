using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Automations
{
    public class AutomationId : IdBase, IEquatable<AutomationId>
    {
        public AutomationId(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Automation ID is invalid.");
        }

        public bool Equals(AutomationId other)
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
