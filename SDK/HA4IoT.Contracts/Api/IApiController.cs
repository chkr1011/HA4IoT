using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Api
{
    public interface IApiController
    {
        void RegisterEndpoint(IApiDispatcherEndpoint endpoint);

        void RouteRequest(string uri, Action<IApiContext> handler);

        void RouteCommand(string uri, Action<IApiContext> handler);

        void NotifyStateChanged(IActuator actuator);
    }
}
