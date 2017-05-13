using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Api
{
    public interface IApiDispatcherService : IService
    {
        event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;

        event EventHandler<ApiRequestReceivedEventArgs> StatusRequestCompleted;

        event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;

        event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequestCompleted;

        void RegisterAdapter(IApiAdapter endpoint);
        
        void Route(string uri, Action<IApiCall> handler);

        void Expose(object controller);
        
        void NotifyStateChanged(IComponent component);
    }
}
