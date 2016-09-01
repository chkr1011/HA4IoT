using System.Globalization;
using HA4IoT.Contracts.Components;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Sensors
{
    public class NumericSensorValue : IComponentState
    {
        private readonly JToken _jsonValue;

        public NumericSensorValue(float value)
        {
            Value = value;
            _jsonValue = JToken.FromObject(Value);
        }

        public float Value { get; }

        public JToken ToJsonValue()
        {
            return _jsonValue;
        }

        public bool Equals(IComponentState otherState)
        {
            var other = otherState as NumericSensorValue;
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return other.Value.Equals(Value);
        }

        public override string ToString()
        {
            return Value.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}
