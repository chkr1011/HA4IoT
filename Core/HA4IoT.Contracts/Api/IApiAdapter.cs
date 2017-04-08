using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Api
{
    public interface IApiAdapter
    {
        event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        void NotifyStateChanged(IComponent component);
    }
}
