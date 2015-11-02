using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public class HttpRequest
    {
        public HttpRequest(HttpMethod method, string uri, string query, IList<HttpHeader> headers, string plainBody, JsonObject jsonBody)
        {
            Method = method;
            Uri = uri;
            Query = query;

            Headers = new ReadOnlyCollection<HttpHeader>(headers);

            PlainBody = plainBody;
            JsonBody = jsonBody;
        }

        public HttpMethod Method { get; }
        public string Uri { get; }
        public string Query { get; }
        public string PlainBody { get; }
        public IReadOnlyCollection<HttpHeader> Headers { get; }

        public JsonObject JsonBody { get; private set; }
    }
}