using System;

namespace HA4IoT.Contracts.Core
{
    public abstract class IdBase
    {
        private readonly string _value;

        protected IdBase(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("The ID '" + value + "' is invalid.");

            _value = value;
        }

        public string Value => _value;

        public override string ToString()
        {
            return _value;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as IdBase;
            if (other == null)
            {
                return false;
            }

            return other.Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
