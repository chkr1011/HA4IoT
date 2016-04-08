using System.Globalization;
using Windows.Data.Json;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public class NumericSensorValue : IComponentState
    {
        private readonly IJsonValue _jsonValue;

        public NumericSensorValue(float value)
        {
            Value = value;
            _jsonValue = JsonValue.CreateNumberValue(Value);
        }

        public float Value { get; }

        public IJsonValue ToJsonValue()
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

            return other.Value.Equals(Value);
        }

        public override string ToString()
        {
            return Value.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}
