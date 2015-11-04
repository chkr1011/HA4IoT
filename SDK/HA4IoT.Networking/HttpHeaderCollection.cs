using System;
using System.Collections.Generic;

namespace HA4IoT.Networking
{
    public class HttpHeaderCollection : List<HttpHeader>
    {
        public void Add(string name, object value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Add(HttpHeader.Create().WithName(name).WithValue(Convert.ToString(value)));
        }

        public bool TryGetValue(string name, out string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            foreach (var header in this)
            {
                if (string.Equals(header.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = header.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
