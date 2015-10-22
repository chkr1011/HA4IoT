using Windows.Data.Json;

namespace CK.HomeAutomation.Networking
{
    public class HttpRequest
    {
        public HttpRequest(HttpMethod method, string uri, string query, string plainBody, JsonObject jsonBody)
        {
            Method = method;
            Uri = uri;
            Query = query;
            PlainBody = plainBody;
            JsonBody = jsonBody;
        }

        public HttpMethod Method { get; }
        public string Uri { get; }
        public string Query { get; }
        public string PlainBody { get; }
        public JsonObject JsonBody { get; private set; }
    }
}