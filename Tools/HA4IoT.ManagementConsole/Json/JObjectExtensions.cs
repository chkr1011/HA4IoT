using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Json
{
    public static class JObjectExtensions
    {
        public static TValue GetNamedValue<TValue>(this JObject jObject, string name, TValue defaultValue)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

            JToken value;
            if (!jObject.TryGetValue(name, out value))
            {
                return defaultValue;
            }

            return value.Value<TValue>();
        }

        public static TValue GetNamedValue<TValue>(this JObject jObject, string name)
        {
            return jObject[name].Value<TValue>();
        }

        public static string GetNamedString(this JObject jObject, string name, string defaultValue)
        {
            return jObject.GetNamedValue(name, defaultValue);
        }

        public static string GetNamedString(this JObject jObject, string name)
        {
            return jObject.GetNamedValue<string>(name);
        }

        public static bool GetNamedBoolean(this JObject jObject, string name, bool defaultValue)
        {
            return jObject.GetNamedValue(name, defaultValue);
        }

        public static bool GetNamedBoolean(this JObject jObject, string name)
        {
            return jObject.GetNamedValue<bool>(name);
        }

        public static decimal GetNamedNumber(this JObject jObject, string name, decimal defaultValue)
        {
            return jObject.GetNamedValue(name, defaultValue);
        }

        public static decimal GetNamedNumber(this JObject jObject, string name)
        {
            return jObject.GetNamedValue<decimal>(name);
        }

        public static JObject GetNamedObject(this JObject jObject, string name, JObject defaultValue)
        {
            return jObject.GetNamedValue(name, defaultValue);
        }

        public static JObject GetNamedObject(this JObject jObject, string name)
        {
            return jObject.GetNamedValue<JObject>(name);
        }

        ////public static JObject GetNamedObject(this JObject jObject, string name, JObject defaultValue)
        ////{

        ////}
    }
}
