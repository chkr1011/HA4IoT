using System;

namespace HA4IoT.Networking
{
    public class HttpRequestDispatcherAction
    {
        public HttpRequestDispatcherAction(HttpMethod method, string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Method = method;
            Uri = uri;
        }

        public Action<HttpContext> Action { get; private set; }
        public HttpMethod Method { get; }
        public string Uri { get; private set; }
        public bool IsJsonBodyRequired { get; private set; }
        public bool HandleRequestsWithDifferentSubUrl { get; private set; }

        public HttpRequestDispatcherAction WithSegment(string value)
        {
            Uri = Uri + "/" + value;
            return this;
        }

        public HttpRequestDispatcherAction Using(Action<HttpContext> action)
        {
            Action = action;
            return this;
        }

        public HttpRequestDispatcherAction WithRequiredJsonBody()
        {
            IsJsonBodyRequired = true;
            return this;
        }

        public HttpRequestDispatcherAction WithAnySubUrl()
        {
            HandleRequestsWithDifferentSubUrl = true;
            return this;
        }
    }
}