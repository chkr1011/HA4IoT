using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.Contracts.Components.States
{
  public abstract class EnumBasedState<TEnum> : IComponentFeatureState
    {
        protected EnumBasedState(TEnum value)
        {
            Value = value;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TEnum Value { get; }

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

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Value.Equals(other.Value);
        }
    }
}
