using System;
using Windows.Data.Json;
using HA4IoT.Networking;

namespace HA4IoT.Contracts.Core
{
    public abstract class IdBase : IConvertibleToJsonValue
    {
        private readonly string _value;

        protected IdBase(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("The ID '" + value + "' is invalid.");

            _value = value;
        }

        public string Value => _value;

        public override string ToString()
        {
            return _value;
        }

        public IJsonValue ToJsonValue()
        {
            return JsonValue.CreateStringValue(_value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
