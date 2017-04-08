using System;
using System.Collections.Generic;
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
            var json = new JObject();
            lock (_values)
            {
                foreach (var value in _values)
                {
                    json[value.Key] = JToken.FromObject(value.Value());
                }
            }

            apiContext.Result = json;
        }
    }
}
