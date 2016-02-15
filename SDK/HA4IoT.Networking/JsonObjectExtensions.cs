using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public static class JsonObjectExtensions
    {
        private static readonly object[] EmptyParameters = new object[0];

        public static JsonObject ToIndexedJsonObject<TKey, TValue>(this Dictionary<TKey, TValue> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            var jsonObject = new JsonObject();
            foreach (var entry in entries)
            {
                jsonObject.SetNamedValue(Convert.ToString(entry.Key), entry.Value.ToJsonValue());    
            }

            return jsonObject;
        }

        public static JsonObject ToJsonObject(this object source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new JsonObject();
            foreach (var property in source.GetType().GetProperties())
            {
                result.SetNamedValue(property.Name, property.GetMethod.Invoke(source, EmptyParameters).ToJsonValue());
            }

            return result;
        }

        public static IJsonValue ToJsonValue(this object source)
        {
            if (source == null)
            {
                return JsonValue.CreateNullValue();
            }

            var stringValue = source as string;
            if (stringValue != null)
            {
                return JsonValue.CreateStringValue(stringValue);
            }

            if (source is Enum)
            {
                return JsonValue.CreateStringValue(source.ToString());
            }

            var boolValue = source as bool?;
            if (boolValue.HasValue)
            {
                return JsonValue.CreateBooleanValue(boolValue.Value);
            }

            var dateTimeValue = source as DateTime?;
            if (dateTimeValue.HasValue)
            {
                return JsonValue.CreateStringValue(dateTimeValue.Value.ToString("O"));
            }

            var convertibleJsonValue = source as IConvertibleToJsonValue;
            if (convertibleJsonValue != null)
            {
                return convertibleJsonValue.ToJsonValue();
            }
            
            var array = source as IEnumerable;
            if (array != null)
            {
                var result = new JsonArray();
                foreach (var arrayEntry in array)
                {
                    result.Add(arrayEntry.ToJsonValue());
                    return result;
                }
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
