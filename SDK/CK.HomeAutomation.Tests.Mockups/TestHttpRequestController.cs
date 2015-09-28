using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Tests.Mockups
{
    public class TestHttpRequestController : IHttpRequestController
    {
        public HttpRequestDispatcherAction Handle(HttpMethod method, string uri)
        {
            return new HttpRequestDispatcherAction(method, uri);
        }
    }
}
