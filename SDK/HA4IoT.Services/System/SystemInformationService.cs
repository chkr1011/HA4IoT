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
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        
        public void Set(string name, string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value;
        }

        public void Set(string name, int? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value;
        }

        public void Set(string name, float? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value;
        }

        public void Set(string name, TimeSpan? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value;
        }

        public void Set(string name, DateTime? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _values[name] = value;
        }

        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = JObject.FromObject(_values);
        }
    }
}
