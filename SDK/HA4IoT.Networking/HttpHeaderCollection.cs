using System;
using System.Collections.Generic;
using System.Linq;

namespace HA4IoT.Networking
{
    public class HttpHeaderCollection : List<HttpHeader>
    {
        public void Add(string name, int value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Add(name, Convert.ToInt32(value));
        }

        public void Add(string name, string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Add(HttpHeader.Create(name).WithValue(value));
        }

        public string GetValue(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            string value;
            if (!TryGetValue(name, out value))
            {
                throw new InvalidOperationException($"HTTP header '{name}' is not set.");
            }

            return value;
        }

        public bool TryGetValue(string name, out string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            HttpHeader header = this.FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (header != null)
            {
                value = header.Value;
                return true;
            }

            value = null;
            return false;
        }
    }
}
