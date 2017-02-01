using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public interface IComponent
    {
        event EventHandler<ComponentStateChangedEventArgs> StateChanged;

        ComponentId Id { get; }

        IList<ComponentState> GetState(); // TODO: Consider create "BrightnessState" with 100% and "PowerState" with "On"

        IList<ComponentState> GetSupportedStates(); // TODO: Consider "SupportedComponentState" class

        void HandleApiCall(IApiContext apiContext);

        JToken ExportConfiguration();

        JToken ExportStatus();
    }
}
