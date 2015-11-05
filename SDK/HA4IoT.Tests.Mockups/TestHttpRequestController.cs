using HA4IoT.Networking;

namespace HA4IoT.Tests.Mockups
{
    public class TestHttpRequestController : IHttpRequestController
    {
        public HttpRequestDispatcherAction Handle(HttpMethod method, string uri)
        {
            return new HttpRequestDispatcherAction(method, uri);
        }
    }
}
