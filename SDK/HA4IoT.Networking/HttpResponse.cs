namespace HA4IoT.Networking
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public IBody Body { get; set; }
    }
}