using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Contracts.Hardware
{
    public class I2CSlaveAddress : IExportToJsonValue
    {
        public I2CSlaveAddress(int value)
        {
            if (value < 0 || value > 127) throw new ArgumentOutOfRangeException(nameof(value), "I2C address is invalid.");
            if (value >= 0x00 && value <= 0x07) throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");
            if (value >= 0x78 && value <= 0x7f) throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");

            Value = value;
        }

        public int Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public IJsonValue ExportToJsonObject()
        {
            return JsonValue.CreateNumberValue(Value);
        }
    }
}
