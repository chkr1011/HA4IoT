using System;

namespace HA4IoT.Networking.Http
{
    public interface IHttpRequestDispatcherAction
    {
        IHttpRequestDispatcherAction Using(Action<HttpContext> handler);
        IHttpRequestDispatcherAction WithAnySubUrl();
    }
}