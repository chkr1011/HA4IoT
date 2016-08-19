using System;
using System.Collections;
using System.Reflection;
using Windows.Data.Json;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking.Json
{
    public static class JsonValueConverter
    {
        public static TObject ToObject<TObject>(this IJsonValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return (TObject)value.ToObject(typeof(TObject));
        }

        public static object ToObject(this IJsonValue value, Type targetType)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (value.GetType() == targetType)
            {
                return value;
            }

            if (value.ValueType == JsonValueType.Null)
            {
                return null;
            }

            if (targetType == typeof(JsonObject))
            {
                return JsonObject.Parse(value.Stringify());
            }

            if (typeof(IJsonValue).IsAssignableFrom(targetType))
            {
                return value;
            }

            if (targetType == typeof(string))
            {
                return value.GetString();
            }

            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                return (int)value.GetNumber();
            }

            if (targetType == typeof(long) || targetType == typeof(long?))
            {
                return (long)value.GetNumber();
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                return value.GetBoolean();
            }

            if (targetType == typeof(float) || targetType == typeof(float?))
            {
                return (float)value.GetNumber();
            }

            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                return value.GetNumber();
            }

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            {
                return (decimal)value.GetNumber();
            }

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                return DateTime.Parse(value.GetString());
            }

            if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?))
            {
                return TimeSpan.Parse(value.GetString());
            }

            throw new NotSupportedException($"Type {targetType} is not supported.");
        }

        public static IJsonValue ToJsonValue(this object source)
        {
            if (source == null)
            {
                return JsonValueCache.NullValue;
            }

            var jsonValue = source as IJsonValue;
            if (jsonValue != null)
            {
                return jsonValue;
            }

            var convertibleJsonValue = source as IExportToJsonValue;
            if (convertibleJsonValue != null)
            {
                return convertibleJsonValue.ExportToJsonObject();
            }

            var stringValue = source as string;
            if (stringValue != null)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    return JsonValueCache.EmptyStringValue;
                }

                return JsonValue.CreateStringValue(stringValue);
            }

            if (source is Enum)
            {
                return JsonValue.CreateStringValue(source.ToString());
            }

            var timeSpan = source as TimeSpan?;
            if (timeSpan.HasValue)
            {
                return JsonValue.CreateStringValue(timeSpan.ToString());
            }

            var boolValue = source as bool?;
            if (boolValue.HasValue)
            {
                return boolValue.Value ? JsonValueCache.TrueValue : JsonValueCache.FalseValue;
            }

            var dateTimeValue = source as DateTime?;
            if (dateTimeValue.HasValue)
            {
                return JsonValue.CreateStringValue(dateTimeValue.Value.ToString("O"));
            }

            var array = source as IEnumerable;
            if (array != null)
            {
                var result = new JsonArray();
                foreach (var arrayEntry in array)
                {
                    result.Add(arrayEntry.ToJsonValue());
                }

                return result;
            }

            if (source is double)
            {
                return JsonValue.CreateNumberValue((double)source);
            }

            if (source is byte)
            {
                return JsonValue.CreateNumberValue(Convert.ToDouble((byte)source));
            }

            if (source is short)
            {
                return JsonValue.CreateNumberValue(Convert.ToDouble((short)source));
            }

            if (source is int)
            {
                return JsonValue.CreateNumberValue(Convert.ToDouble((int)source));
            }

            if (source is long)
            {
                return JsonValue.CreateNumberValue(Convert.ToDouble((long)source));
            }

            if (source is float)
            {
                return JsonValue.CreateNumberValue(Convert.ToDouble((float)source));
            }

            if (source is decimal)
            {
                return JsonValue.CreateNumberValue(Convert.ToDouble((decimal)source));
            }

            return source.ToJsonObject();
        }
    }
}
