using Windows.Data.Json;

namespace HA4IoT.Contracts.Sensors
{
    public class NumericSensorValue : ISensorValue
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
    }
}
