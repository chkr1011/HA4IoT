using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public class ComponentState
    {
        private readonly int _hashCode;

        public ComponentState(object state)
        {
            JToken = new JValue(state);
            _hashCode = JToken.ToString().GetHashCode();
        }

        public JToken JToken { get; }

        public TValue ToObject<TValue>()
        {
            return JToken.ToObject<TValue>();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object @object)
        {
            var other = @object as ComponentState;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public bool Equals(ComponentState componentState)
        {
            if (ReferenceEquals(componentState, this))
            {
                return true;
            }

            if (componentState == null)
            {
                return false;
            }

            return JToken.DeepEquals(JToken, componentState.JToken);
        }

        public override string ToString()
        {
            return Convert.ToString(JToken);
        }
    }
}
