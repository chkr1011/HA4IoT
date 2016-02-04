using System;
using System.Collections;
using System.Collections.Generic;

namespace HA4IoT.Networking
{
    public class HttpHeaderCollection : Dictionary<string, string>
    {
        public HttpHeaderCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
