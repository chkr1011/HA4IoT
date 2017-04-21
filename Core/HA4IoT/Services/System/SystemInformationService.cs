using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.System
{
    [ApiServiceClass(typeof(ISystemInformationService))]
    public class SystemInformationService : ServiceBase, ISystemInformationService
    {
        private readonly Dictionary<string, Func<object>> _values = new Dictionary<string, Func<object>>();
        
        public void Set(string name, object value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_values)
            {
                _values[name] = () => value;
            }
        }

        public void Set(string name, Func<object> valueProvider)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));

            lock (_values)
            {
                _values[name] = valueProvider;
            }
        }

        [ApiMethod]
        public void GetStatus(IApiContext apiContext)
        {
            Dictionary<string, Func<object>> values;
            lock (_values)
            {
                values = new Dictionary<string, Func<object>>(_values);
            }

            var json = new JObject();
            foreach (var value in values)
            {
                var effectiveValue = value.Value();
                if (effectiveValue == null)
                {
                    json[value.Key] = JValue.CreateNull();
                }
                else
                {
                    json[value.Key] = JToken.FromObject(value.Value());
                }
            }

            apiContext.Result = json;
        }
    }
}
