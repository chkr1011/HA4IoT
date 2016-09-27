using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Api
{
    public interface IApiService : IService
    {
        event EventHandler<ApiRequestReceivedEventArgs> StatusRequested;

        event EventHandler<ApiRequestReceivedEventArgs> StatusRequestCompleted;

        event EventHandler<ApiRequestReceivedEventArgs> ConfigurationRequested;  

        void RegisterEndpoint(IApiDispatcherEndpoint endpoint);
        
        void Route(string uri, Action<IApiContext> handler);

        void Expose(object controller);
        
        void NotifyStateChanged(IComponent component);
    }
}
