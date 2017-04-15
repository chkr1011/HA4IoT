using System;
using System.Collections.Generic;
using Windows.Web.Http;

namespace HA4IoT.Net.Http
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Ok;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public byte[] Body { get; set; }
        public string MimeType { get; set; }
    }
}