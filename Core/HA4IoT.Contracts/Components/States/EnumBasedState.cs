using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.States
{
  public abstract class EnumBasedState<TEnum> : IComponentFeatureState
    {
        protected EnumBasedState(TEnum value)
        {
            Value = value;
        }

        public TEnum Value { get; }

        public JToken Serialize()
        {
            return JToken.FromObject(Value.ToString());
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as EnumBasedState<TEnum>;
            if (other == null)
            {
                return false;
            }

            return Value.Equals(other.Value);
        }
    }
}
