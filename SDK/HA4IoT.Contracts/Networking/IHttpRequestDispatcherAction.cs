using System;

namespace HA4IoT.Contracts.Networking
{
    public interface IHttpRequestDispatcherAction
    {
        IHttpRequestDispatcherAction Using(Action<HttpContext> handler);
        IHttpRequestDispatcherAction WithAnySubUrl();
    }
}