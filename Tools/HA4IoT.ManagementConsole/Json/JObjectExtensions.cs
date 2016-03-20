using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Json
{
    public static class JObjectExtensions
    {
        public static TValue GetNamedValue<TValue>(this JObject jObject, string name, TValue defaultValue)
        {
            if (jObject == null)
            {
                return defaultValue;
            }

            JToken value;
            if (!jObject.TryGetValue(name, out value))
            {
                return defaultValue;
            }

            return value.Value<TValue>();
        }

        public static JArray GetNamedArray(this JObject jObject, string name)
        {
            return (JArray) jObject[name];
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

        public static void SetNamedString(this JObject jObject, string name, string value)
        {
            jObject[name] = new JValue(value);
        }

        public static bool GetNamedBoolean(this JObject jObject, string name, bool defaultValue)
        {
            return jObject.GetNamedValue(name, defaultValue);
        }

        public static void SetNamedBoolean(this JObject jObject, string name, bool value)
        {
            jObject[name] = new JValue(value);
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

        public static void SetNamedNumber(this JObject jObject, string name, decimal value)
        {
            jObject[name] = new JValue(value);
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
