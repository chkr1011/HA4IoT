using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Tests.Mockups
{
    public class TestHttpRequestController : IHttpRequestController
    {
        public IHttpRequestDispatcherAction HandleGet(string uri)
        {
            return new HttpRequestDispatcherAction(HttpMethod.Get, uri);
        }

        public IHttpRequestDispatcherAction HandlePost(string uri)
        {
            return new HttpRequestDispatcherAction(HttpMethod.Post, uri);
        }

        public IHttpRequestDispatcherAction HandlePatch(string uri)
        {
            return new HttpRequestDispatcherAction(HttpMethod.Patch, uri);
        }
    }
}
