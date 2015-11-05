using System;
using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public class HttpRequest
    {
        public HttpRequest(HttpMethod method, string uri, string query, HttpHeaderCollection headers, string plainBody, JsonObject jsonBody)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (plainBody == null) throw new ArgumentNullException(nameof(plainBody));

            Method = method;
            Uri = uri;
            Query = query;

            Headers = headers;

            PlainBody = plainBody;
            JsonBody = jsonBody;
        }

        public HttpMethod Method { get; }
        public string Uri { get; }
        public string Query { get; }
        public string PlainBody { get; }
        public HttpHeaderCollection Headers { get; }

        public JsonObject JsonBody { get; private set; }
    }
}