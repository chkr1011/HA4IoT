namespace CK.HomeAutomation.Networking
{
    public interface IHttpRequestController
    {
        HttpRequestDispatcherAction Handle(HttpMethod method, string uri);
    }
}
