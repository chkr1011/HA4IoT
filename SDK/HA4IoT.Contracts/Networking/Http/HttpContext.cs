using System;

namespace HA4IoT.Contracts.Networking.Http
{
    public class HttpContext
    {
        public HttpContext(HttpRequest request, HttpResponse response)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            Request = request;
            Response = response;
        }

        public HttpRequest Request { get; }
        public HttpResponse Response { get; }
    }
}