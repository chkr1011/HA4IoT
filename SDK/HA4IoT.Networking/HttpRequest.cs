using System;

namespace HA4IoT.Networking
{
    public class HttpRequest
    {
        public HttpRequest(HttpMethod method, string uri, string query, HttpHeaderCollection headers, string body)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (body == null) throw new ArgumentNullException(nameof(body));

            Method = method;
            Uri = uri;
            Query = query;

            Headers = headers;
            Body = body;
        }

        public HttpMethod Method { get; }
        public string Uri { get; }
        public string Query { get; }
        public string Body { get; }
        public HttpHeaderCollection Headers { get; }
    }
}