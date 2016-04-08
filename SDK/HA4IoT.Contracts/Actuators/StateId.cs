using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public class StateId : IdBase, IEquatable<StateId>, IComponentState
    {
        private readonly IJsonValue _jsonValue;

        public StateId(string value) 
            : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("State ID is invalid.");

            _jsonValue = JsonValue.CreateStringValue(value);
        }

        public bool Equals(StateId other)
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

        public bool Equals(IComponentState otherState)
        {
            var other = otherState as StateId;
            if (other == null)
            {
                return false;
            }

            return other.Value.Equals(Value);
        }

        public IJsonValue ToJsonValue()
        {
            return _jsonValue;
        }
    }
}
