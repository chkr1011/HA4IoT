using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Windows.Data.Json;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking.Json
{
    public static class JsonObjectSerializer
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

        public static JsonObject ToJsonObject(this object source, ToJsonObjectMode mode = ToJsonObjectMode.Implicit)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new JsonObject();
            foreach (var property in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (mode == ToJsonObjectMode.Explicit)
                {
                    if (property.GetCustomAttribute<JsonMemberAttribute>() == null)
                    {
                        continue;
                    }
                }
                else if (mode == ToJsonObjectMode.Implicit)
                {
                    if (property.GetCustomAttribute<NoJsonMemberAttribute>() != null)
                    {
                        continue;
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
                
                var value = property.GetValue(source).ToJsonValue();
                result.SetNamedValue(property.Name, value);
            }

            return result;
        }
        
        public static void DeserializeTo(this JsonObject jsonObject, object target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var properties = target.GetType().GetProperties();
            var importFromJsonType = typeof(IImportFromJsonValue);

            foreach (var property in properties)
            {
                IJsonValue jsonValue;
                if (!jsonObject.TryGetValue(property.Name, out jsonValue))
                {
                    continue;
                }

                if (importFromJsonType.IsAssignableFrom(property.PropertyType))
                {
                    var propertyValue = (IImportFromJsonValue)property.GetValue(target);
                    propertyValue?.ImportFromJsonValue(jsonValue);

                    continue;
                }

                object value = jsonValue.ToObject(property.PropertyType);
                property.SetValue(target, value);
            }
        }

        public static byte[] ToByteArray(this JsonObject jsonObject)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));

            return Encoding.UTF8.GetBytes(jsonObject.Stringify());
        }
    }
}
