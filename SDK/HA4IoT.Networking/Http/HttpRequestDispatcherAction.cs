using System;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public class HttpRequestDispatcherAction : IHttpRequestDispatcherAction
    {
        public HttpRequestDispatcherAction(HttpMethod method, string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Method = method;
            Uri = uri;
        }

        public HttpMethod Method { get; }
        public string Uri { get; private set; }
        public bool HandleRequestsWithDifferentSubUrl { get; private set; }
        public Action<HttpContext> Handler { get; private set; }
        
        public IHttpRequestDispatcherAction Using(Action<HttpContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Handler = handler;
            return this;
        }

        public IHttpRequestDispatcherAction WithAnySubUrl()
        {
            HandleRequestsWithDifferentSubUrl = true;
            return this;
        }
    }
}