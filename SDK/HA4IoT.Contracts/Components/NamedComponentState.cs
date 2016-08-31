using System;
using HA4IoT.Contracts.Core;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public class NamedComponentState : IdBase, IEquatable<NamedComponentState>, IComponentState
    {
        private readonly JToken _jsonValue;

        public NamedComponentState(string name) 
            : base(name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name is invalid.");

            _jsonValue = JValue.CreateString(name);
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

        public JToken ToJsonValue()
        {
            return _jsonValue;
        }
    }
}
