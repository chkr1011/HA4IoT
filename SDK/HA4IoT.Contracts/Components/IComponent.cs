using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Components
{
    public interface IComponent
    {
        event EventHandler<ComponentStateChangedEventArgs> StateChanged;

        ComponentId Id { get; }

        ISettingsContainer Settings { get; }

        IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        IComponentState GetState();

        IList<IComponentState> GetSupportedStates();

        void HandleApiCall(IApiContext apiContext);

        JsonObject ExportConfigurationToJsonObject();

        JsonObject ExportStatusToJsonObject();
    }
}
