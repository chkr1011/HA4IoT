using System;

namespace HA4IoT.Contracts.Networking
{
    public interface IHttpRequestController
    {
        IHttpRequestDispatcherAction HandleGet(string uri);

        IHttpRequestDispatcherAction HandlePost(string uri);

        IHttpRequestDispatcherAction HandlePatch(string uri);
    }
}
