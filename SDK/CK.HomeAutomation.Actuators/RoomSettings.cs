using System;
using System.Collections.Generic;
using Windows.Data.Json;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Actuators
{
    public class RoomSettings
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public RoomSettings(Room room, HttpRequestDispatcher httpRequestDispatcher)
        {
           // httpRequestDispatcher.GetController("api").Handle(HttpMethod.Get, "rootSettings").WithValue(room.Id).Using(GetValuesForApi);
        }

        public bool GetBool(string name)
        {
            return (bool) _values[name];
        }

        public int GetInt32(string name)
        {
            return (int) _values[name];
        }

        public string GetString(string name)
        {
            return (string) _values[name];
        }

        public TimeSpan? GetTimeSpan(string name)
        {
            return (TimeSpan?) _values[name];
        }

        //public void Map(string name, Action<RoomSettings, string> connector)
        //{
        //}

        public void Set(string name, object value)
        {
            GetJsonValue(value);
            _values.Add(name, value);
        }

        public void SetDefault(string name, object value)
        {
            if (_values.ContainsKey(name))
            {
                return;
            }

            Set(name, value);
        }

        private void GetValuesForApi(HttpContext context)
        {
            var values = new JsonObject();
            foreach (var value in _values)
            {
                values[value.Key] = GetJsonValue(value.Value);
            }
        }

        private JsonValue GetJsonValue(object value)
        {
            if (value == null)
            {
                return JsonValue.CreateNullValue();
            }

            if (value is int)
            {
                return JsonValue.CreateNumberValue((int)value);
            }

            if (value is bool)
            {
                return JsonValue.CreateBooleanValue((bool)value);
            }

            if (value is string)
            {
                return JsonValue.CreateStringValue((string)value);
            }

            if (value is TimeSpan?)
            {
                return JsonValue.CreateStringValue(((TimeSpan?)value).ToString());
            }

            throw new NotSupportedException();
        }
    }
}