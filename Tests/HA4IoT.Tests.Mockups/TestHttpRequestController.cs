using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Tests.Mockups
{
    public class TestHttpRequestController : IHttpRequestController, IApiService
    {
        public event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;

        public event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;

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

        public void RouteRequest(string uri, Action<IApiContext> handler)
        {
        }

        public void RouteCommand(string uri, Action<IApiContext> handler)
        {
        }

        public void Route(string uri, Action<IApiContext> handler)
        {
        }

        public void Expose(string baseUri, object controller)
        {
        }

        public void NotifyStateChanged(IComponent component)
        {
        }

        public void RegisterEndpoint(IApiDispatcherEndpoint endpoint)
        {
        }
    }
}
