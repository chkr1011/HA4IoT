using System;
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

        void HandleApiCommand(IApiContext apiContext);

        void HandleApiRequest(IApiContext apiContext);

        JsonObject ExportConfigurationToJsonObject();

        JsonObject ExportStatusToJsonObject();
    }
}
