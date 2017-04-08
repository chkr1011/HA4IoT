using System;

namespace HA4IoT.Networking.Http.Controllers
{
    public class HttpRequestControllerAction
    {
        public HttpRequestControllerAction(HttpMethod method, string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Method = method;
            Uri = uri;
        }

        public HttpMethod Method { get; }
        public string Uri { get; private set; }
        public bool HandleRequestsWithDifferentSubUrl { get; private set; }
        public Action<HttpContext> Handler { get; private set; }
        
        public HttpRequestControllerAction Using(Action<HttpContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Handler = handler;
            return this;
        }

        public HttpRequestControllerAction WithAnySubUrl()
        {
            HandleRequestsWithDifferentSubUrl = true;
            return this;
        }
    }
}