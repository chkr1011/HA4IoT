using System;

namespace HA4IoT.Contracts.Api
{
    public interface IApiController
    {
        void RouteRequest(string uri, Action<IApiContext> handler);

        void RouteCommand(string uri, Action<IApiContext> handler);

        void RegisterEndpoint(IApiDispatcherEndpoint endpoint);
    }
}
