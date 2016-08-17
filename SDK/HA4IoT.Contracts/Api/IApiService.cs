using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Api
{
    public interface IApiService
    {
        event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;

        event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;  

        void RegisterEndpoint(IApiDispatcherEndpoint endpoint);

        void RouteRequest(string uri, Action<IApiContext> handler);

        void RouteCommand(string uri, Action<IApiContext> handler);

        void Route(string uri, Action<IApiContext> handler);

        void Expose(string baseUri, object controller);

        void NotifyStateChanged(IComponent component);
    }
}
