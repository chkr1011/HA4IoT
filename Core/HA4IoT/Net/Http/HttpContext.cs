using System;

namespace HA4IoT.Net.Http
{
    public class HttpContext
    {
        public HttpContext(HttpRequest request, HttpResponse response)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public HttpRequest Request { get; }
        public HttpResponse Response { get; }
    }
}