using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Contracts.Components
{
    public interface IComponent
    {
        event EventHandler<ComponentStateChangedEventArgs> StateChanged;

        ComponentId Id { get; }

        IComponentState GetState();

        // TODO: ToIJsonValue
        IList<IComponentState> GetSupportedStates();

        void HandleApiCall(IApiContext apiContext);

        JsonObject ExportConfiguration();

        JsonObject ExportStatus();
    }
}
