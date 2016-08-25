using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Json;

namespace HA4IoT.Services.System
{
    [ApiServiceClass(typeof(ISystemInformationService))]
    public class SystemInformationService : ServiceBase, ISystemInformationService
    {
        private readonly Dictionary<string, IJsonValue> _values = new Dictionary<string, IJsonValue>();
        
        public void Set(string name, string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value.ToJsonValue();
        }

        public void Set(string name, int? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value.ToJsonValue();
        }

        public void Set(string name, float? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value.ToJsonValue();
        }

        public void Set(string name, TimeSpan? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value.ToJsonValue();
        }

        public void Set(string name, DateTime? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value.ToJsonValue();
        }

        [ApiMethod(ApiCallType.Request)]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = new JsonObject();

            foreach (var staticValue in _values.OrderBy(v => v.Key))
            {
                apiContext.Response.SetNamedValue(staticValue.Key, staticValue.Value);
            }
        }
    }
}
