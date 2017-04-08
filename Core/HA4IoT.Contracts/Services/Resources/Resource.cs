using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HA4IoT.Contracts.Services.Resources
{
    public class Resource
    {
        public Resource(string uri, string defaultValue)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (defaultValue == null) throw new ArgumentNullException(nameof(defaultValue));

            Uri = uri;
            DefaultValue = defaultValue;
        }

        [JsonRequired]
        public string Uri { get; }

        [JsonRequired]
        public string DefaultValue { get; }

        [JsonRequired]
        public List<ResourceValue> Values { get; } = new List<ResourceValue>();
    }
}
