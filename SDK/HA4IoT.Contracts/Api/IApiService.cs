using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Api
{
    public interface IApiService : IService
    {
        event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;

        event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;  

        void RegisterEndpoint(IApiDispatcherEndpoint endpoint);

        void RouteRequest(string uri, Action<IApiContext> handler);

        void RouteCommand(string uri, Action<IApiContext> handler);

        void Route(string uri, Action<IApiContext> handler);

        void Expose(object controller);

        void Expose(string baseUri, object controller);

        void NotifyStateChanged(IComponent component);
    }
}
