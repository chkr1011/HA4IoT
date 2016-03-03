using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Contracts.Core
{
    public abstract class IdBase : IExportToJsonValue
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

        public IJsonValue ExportToJsonObject()
        {
            return JsonValue.CreateStringValue(_value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
