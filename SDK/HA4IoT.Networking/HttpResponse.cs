using System.Collections.Generic;

namespace HA4IoT.Networking
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public IBody Body { get; set; }
        public HttpHeaderCollection Headers { get; } = new HttpHeaderCollection();
    }
}