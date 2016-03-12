using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Api
{
    public interface IApiDispatcherEndpoint
    {
        event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        void NotifyStateChanged(IActuator actuator);
    }
}
