namespace HA4IoT.Contracts.Networking.Http
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public IHttpBody Body { get; set; }
        public HttpHeaderCollection Headers { get; } = new HttpHeaderCollection();
    }
}