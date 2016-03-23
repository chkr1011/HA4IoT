using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Windows.Data.Json;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking
{
    public static class JsonObjectExtensions
    {
        private static readonly IJsonValue NullValue = JsonValue.CreateNullValue();

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
                if (property.GetCustomAttribute<HideFromToJsonObjectAttribute>() != null)
                {
                    continue;
                }

                IJsonValue value = property.GetValue(source).ToJsonValue();
                result.SetNamedValue(property.Name, value);
            }

            return result;
        }

        public static IJsonValue ToJsonValue(this object source)
        {
            if (source == null)
            {
                return NullValue;
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
                return JsonValue.CreateBooleanValue(boolValue.Value);
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

        public static TObject ToObject<TObject>(this IJsonValue value)
        {
            return (TObject)value.ToObject(typeof(TObject));
        }

        public static object ToObject(this IJsonValue value, Type targetType)
        {
            if (value.GetType() == targetType)
            {
                return value;
            }
            
            if (value.ValueType == JsonValueType.Null)
            {
                return null;
            }

            if (targetType == typeof (JsonObject))
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

        public static void SetNamedString(this JsonObject jsonObject, string name, string value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (value == null)
            {
                jsonObject.SetNamedValue(name, JsonValue.CreateNullValue());
            }
            else
            {
                jsonObject.SetNamedValue(name, JsonValue.CreateStringValue(value));
            }
        }

        public static void SetNamedNumber(this JsonObject jsonObject, string name, double value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, JsonValue.CreateNumberValue(value));
        }

        public static void SetNamedBoolean(this JsonObject jsonObject, string name, bool value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, JsonValue.CreateBooleanValue(value));
        }

        public static void SetNamedNullValue(this JsonObject jsonObject, string name)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, JsonValue.CreateNullValue());
        }

        public static void SetNamedObject(this JsonObject jsonObject, string name, JsonObject value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (value == null)
            {
                jsonObject.SetNamedValue(name, JsonValue.CreateNullValue());
            }
            else
            {
                jsonObject.SetNamedValue(name, value);
            }
        }

        public static void SetNamedArray(this JsonObject jsonObject, string name, JsonArray array)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedValue(name, array);
        }

        public static void SetNamedDateTime(this JsonObject jsonObject, string name, DateTime? value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (!value.HasValue)
            {
                jsonObject.SetNamedNullValue(name);
            }
            else
            {
                jsonObject.SetNamedString(name, value.Value.ToString("O"));
            }
        }

        public static void SetNamedTimeSpan(this JsonObject jsonObject, string name, TimeSpan? value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (!value.HasValue)
            {
                jsonObject.SetNamedNullValue(name);
            }
            else
            {
                jsonObject.SetNamedString(name, value.Value.ToString("c"));
            }
        }

        public static JsonObject WithNamedString(this JsonObject jsonObject, string name, string value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedString(name, value);
            return jsonObject;
        }

        public static JsonObject WithNamedNumber(this JsonObject jsonObject, string name, double value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedNumber(name, value);
            return jsonObject;
        }

        public static JsonObject WithNamedBoolean(this JsonObject jsonObject, string name, bool value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedBoolean(name, value);
            return jsonObject;
        }

        public static JsonObject WithNamedNullValue(this JsonObject jsonObject, string name)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedNullValue(name);
            return jsonObject;
        }

        public static JsonObject WithNamedObject(this JsonObject jsonObject, string name, JsonObject value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedObject(name, value);
            return jsonObject;
        }

        public static JsonObject WithNamedArray(this JsonObject jsonObject, string name, JsonArray value)
        {
            if (jsonObject == null) throw new ArgumentNullException(nameof(jsonObject));
            if (name == null) throw new ArgumentNullException(nameof(name));

            jsonObject.SetNamedArray(name, value);
            return jsonObject;
        }
    }
}
