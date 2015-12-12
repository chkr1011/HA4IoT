using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public static class JsonObjectExtensions
    {
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
            
            if (source is byte || source is short || source is int || source is long || source is float || source is double || source is decimal)
            {
                return JsonValue.CreateNumberValue((double) source);
            }

            return JsonValue.CreateStringValue(Convert.ToString(source));
        }
    }
}
