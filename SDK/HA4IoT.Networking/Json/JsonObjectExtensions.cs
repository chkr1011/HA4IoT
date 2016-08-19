using System;
using System.Globalization;
using Windows.Data.Json;

namespace HA4IoT.Networking.Json
{
    public static class JsonObjectExtensions
    {
        public static void Import(this JsonObject target, JsonObject source)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));

            foreach (var property in source.Keys)
            {
                var sourceValue = source[property];

                if (!target.ContainsKey(property))
                {
                    target[property] = sourceValue;
                }
                else
                {
                    var targetValue = target[property];

                    if (targetValue.ValueType == sourceValue.ValueType && targetValue.ValueType != JsonValueType.Object)
                    {
                        target[property] = sourceValue;
                    }
                    else
                    {
                        if (targetValue.ValueType != JsonValueType.Object ||
                            sourceValue.ValueType != JsonValueType.Object)
                        {
                            throw new ImportNotPossibleException();
                        }

                        Import(targetValue.GetObject(), sourceValue.GetObject());
                    }
                }
            }
        }

        public static void SetValue(this JsonObject jsonObject, string name, string value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (value == null)
            {
                jsonObject.SetNamedValue(name, JsonValueCache.NullValue);
            }
            else if (value == string.Empty)
            {
                jsonObject.SetNamedValue(name, JsonValueCache.EmptyStringValue);
            }
            else
            {
                jsonObject.SetNamedValue(name, JsonValue.CreateStringValue(value));
            }
        }

        public static void SetValue(this JsonObject jsonObject, string name, double? value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (!value.HasValue)
            {
                jsonObject.SetNamedValue(name, JsonValueCache.NullValue);
            }
            else
            {
                jsonObject.SetNamedValue(name, JsonValue.CreateNumberValue(value.Value));
            }
        }

        public static void SetValue(this JsonObject jsonObject, string name, Enum value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, JsonValue.CreateStringValue(value.ToString()));
        }

        public static void SetValue(this JsonObject jsonObject, string name, bool value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, value ? JsonValueCache.TrueValue : JsonValueCache.FalseValue);
        }

        public static void SetValue(this JsonObject jsonObject, string name)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, JsonValueCache.NullValue);
        }

        public static JsonObject SetValue(this JsonObject jsonObject, string name, JsonObject value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (value == null)
            {
                jsonObject.SetNamedValue(name, JsonValueCache.NullValue);
            }
            else
            {
                jsonObject.SetNamedValue(name, value);
            }

            return value;
        }

        public static JsonArray SetValue(this JsonObject jsonObject, string name, JsonArray array)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, array);

            return array;
        }

        public static void SetValue(this JsonObject jsonObject, string name, DateTime? value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (!value.HasValue)
            {
                jsonObject.SetValue(name);
            }
            else
            {
                jsonObject.SetValue(name, value.Value.ToString("O"));
            }
        }

        public static void SetValue(this JsonObject jsonObject, string name, TimeSpan? value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (!value.HasValue)
            {
                jsonObject.SetValue(name);
            }
            else
            {
                jsonObject.SetValue(name, value.Value.ToString("c"));
            }
        }

        public static TimeSpan? GetTimeSpan(this JsonObject jsonObject, string name)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var value = jsonObject.GetNamedValue(name);
            if (value.ValueType == JsonValueType.Null)
            {
                return null;
            }

            return TimeSpan.ParseExact(value.GetString(), "c", DateTimeFormatInfo.InvariantInfo);
        }

        public static JsonObject WithString(this JsonObject jsonObject, string name, string value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetValue(name, value);
            return jsonObject;
        }

        public static JsonObject WithNumber(this JsonObject jsonObject, string name, double value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetValue(name, value);
            return jsonObject;
        }

        public static JsonObject WithBoolean(this JsonObject jsonObject, string name, bool value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetValue(name, value);
            return jsonObject;
        }

        public static JsonObject WithNullValue(this JsonObject jsonObject, string name)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetValue(name);
            return jsonObject;
        }

        public static JsonObject WithObject(this JsonObject jsonObject, string name, JsonObject value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetValue(name, value);
            return jsonObject;
        }

        public static JsonObject WithArray(this JsonObject jsonObject, string name, JsonArray value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetValue(name, value);
            return jsonObject;
        }
    }
}
