using Windows.Data.Json;

namespace HA4IoT.Networking.Json
{
    public static class JsonValueCache
    {
        public static readonly IJsonValue NullValue = JsonValue.CreateNullValue();
        public static readonly IJsonValue EmptyStringValue = JsonValue.CreateStringValue(string.Empty);
        public static readonly IJsonValue TrueValue = JsonValue.CreateBooleanValue(true);
        public static readonly IJsonValue FalseValue = JsonValue.CreateBooleanValue(false);
        public static readonly IJsonValue ZeroValue = JsonValue.CreateNumberValue(0);
    }
}
