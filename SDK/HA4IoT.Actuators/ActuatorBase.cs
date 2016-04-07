using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : IActuator
    {
        private IApiController _apiController;

        protected ActuatorBase(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;

            Settings = new SettingsContainer(StoragePath.WithFilename("Actuators", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new ActuatorSettingsWrapper(Settings);
        }

        public event EventHandler<StateChangedEventArgs> ActiveStateChanged;

        public ComponentId Id { get; }

        public ISettingsContainer Settings { get; }

        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedObject("settings", Settings.Export());
            result.SetNamedString("state", GetActiveState().Value);

            return result;
        }

        public abstract StateId GetActiveState();
        
        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedValue("type", GetType().FullName.ToJsonValue());
            result.SetNamedValue("settings", Settings.Export());

            return result;
        }

        public void ExposeToApi(IApiController apiController)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            new ActuatorSettingsApiDispatcher(this, apiController).ExposeToApi();

            apiController.RouteCommand($"actuator/{Id}/status", HandleApiCommand);
            apiController.RouteRequest($"actuator/{Id}/status", HandleApiRequest);

            _apiController = apiController;
        }

        protected virtual void HandleApiCommand(IApiContext apiContext)
        {
        }

        protected virtual void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = ExportStatusToJsonObject();
        }

        protected void OnActiveStateChanged(StateId oldState)
        {
            StateId newState = GetActiveState();
            Log.Info($"Actuator '{Id}' updated state from '{oldState}' to '{newState}'");

            _apiController?.NotifyStateChanged(this);
            ActiveStateChanged?.Invoke(this, new StateChangedEventArgs(oldState, newState));
        }
    }
}