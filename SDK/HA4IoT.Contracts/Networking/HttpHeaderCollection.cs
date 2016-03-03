using System;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Networking
{
    public class HttpHeaderCollection : Dictionary<string, string>
    {
        public HttpHeaderCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
