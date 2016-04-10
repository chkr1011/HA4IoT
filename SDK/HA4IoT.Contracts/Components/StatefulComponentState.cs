using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Components
{
    public class StatefulComponentState : IdBase, IEquatable<StatefulComponentState>, IComponentState
    {
        private readonly IJsonValue _jsonValue;

        public StatefulComponentState(string value) 
            : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("State ID is invalid.");

            _jsonValue = JsonValue.CreateStringValue(value);
        }

        public bool Equals(StatefulComponentState otherState)
        {
            if (ReferenceEquals(otherState, this))
            {
                return true;
            }

            if (otherState == null)
            {
                return false;
            }
            
            return Value.Equals(otherState.Value);
        }

        public bool Equals(IComponentState otherState)
        {
            if (ReferenceEquals(otherState, this))
            {
                return true;
            }

            return Equals(otherState as StatefulComponentState);
        }

        public IJsonValue ToJsonValue()
        {
            return _jsonValue;
        }
    }
}
