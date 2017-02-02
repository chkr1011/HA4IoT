using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public class GenericComponentState : IComponentFeatureState
    {
        public GenericComponentState(object state)
        {
            JToken = new JValue(state);
        }

        public JToken JToken { get; }

        public override int GetHashCode()
        {
            return JToken.ToString().GetHashCode();
        }

        public override bool Equals(object @object)
        {
            var other = @object as GenericComponentState;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public JToken Serialize()
        {
            return JToken;
        }

        public bool Equals(GenericComponentState componentState)
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
