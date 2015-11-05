namespace HA4IoT.Networking
{
    public interface IHttpRequestController
    {
        HttpRequestDispatcherAction Handle(HttpMethod method, string uri);
    }
}
