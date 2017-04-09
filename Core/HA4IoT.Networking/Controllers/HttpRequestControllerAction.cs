using System;
using Windows.Web.Http;
using HA4IoT.Networking.Http;

namespace HA4IoT.Networking.Controllers
{
    public class HttpRequestControllerAction
    {
        public HttpRequestControllerAction(HttpMethod method, string uri)
        {
            Method = method;
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public HttpMethod Method { get; }
        public string Uri { get; }
        public bool HandleRequestsWithDifferentSubUrl { get; private set; }
        public Action<HttpContext> Handler { get; private set; }
        
        public HttpRequestControllerAction Using(Action<HttpContext> handler)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        public HttpRequestControllerAction WithAnySubUrl()
        {
            HandleRequestsWithDifferentSubUrl = true;
            return this;
        }
    }
}