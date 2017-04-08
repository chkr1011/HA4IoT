using System;
using System.Collections.Generic;

namespace HA4IoT.Networking.Http
{
    public class HttpRequest
    {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string Uri { get; set; }
        public Version HttpVersion { get; set; } = new Version(1, 1);
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public byte[] Body { get; set; }

        public string Query { get; set; }
    }
}