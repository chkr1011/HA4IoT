using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Components
{
    public class NamedComponentState : IdBase, IEquatable<NamedComponentState>, IComponentState
    {
        private readonly IJsonValue _jsonValue;

        public NamedComponentState(string name) 
            : base(name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name is invalid.");

            _jsonValue = JsonValue.CreateStringValue(name);
        }

        public bool Equals(NamedComponentState otherState)
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

            return Equals(otherState as NamedComponentState);
        }

        public IJsonValue ToJsonValue()
        {
            return _jsonValue;
        }
    }
}
