using System;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Networking.Http
{
    public class HttpHeaderCollection : Dictionary<string, string>
    {
        public HttpHeaderCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public bool ValueEquals(string key, string expectedValue)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (expectedValue == null) throw new ArgumentNullException(nameof(expectedValue));

            string value;
            if (!TryGetValue(key, out value))
            {
                return false;
            }

            return string.Equals(value, expectedValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
