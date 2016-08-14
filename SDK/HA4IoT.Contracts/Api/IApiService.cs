using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Api
{
    public interface IApiService
    {
        void RegisterEndpoint(IApiDispatcherEndpoint endpoint);

        void RouteRequest(string uri, Action<IApiContext> handler);

        void RouteCommand(string uri, Action<IApiContext> handler);

        void NotifyStateChanged(IComponent component);
    }
}
