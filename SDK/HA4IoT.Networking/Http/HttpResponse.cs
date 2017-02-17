using System;
using System.Collections.Generic;
using System.Net;

namespace HA4IoT.Networking.Http
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public IHttpBody Body { get; set; }
    }
}