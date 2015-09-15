namespace CK.HomeAutomation.Networking
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public IBody Body { get; set; }
    }
}