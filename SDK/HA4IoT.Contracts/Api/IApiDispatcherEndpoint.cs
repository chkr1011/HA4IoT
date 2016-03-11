using System;

namespace HA4IoT.Contracts.Api
{
    public interface IApiDispatcherEndpoint
    {
        event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;
    }
}
