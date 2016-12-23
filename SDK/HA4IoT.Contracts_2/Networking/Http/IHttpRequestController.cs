namespace HA4IoT.Contracts.Networking.Http
{
    public interface IHttpRequestController
    {
        IHttpRequestDispatcherAction HandleGet(string uri);

        IHttpRequestDispatcherAction HandlePost(string uri);

        IHttpRequestDispatcherAction HandlePatch(string uri);
    }
}
