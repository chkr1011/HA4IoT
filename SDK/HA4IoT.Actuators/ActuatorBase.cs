using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : IActuator, IStatusProvider
    {
        private IApiController _apiController;

        protected ActuatorBase(ActuatorId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;

            Settings = new SettingsContainer(StoragePath.WithFilename("Actuators", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new ActuatorSettingsWrapper(Settings);
        }

        public ActuatorId Id { get; }

        public ISettingsContainer Settings { get; }

        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            return Settings.Export();
        }

        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedValue("Type", GetType().FullName.ToJsonValue());

            if (Settings != null)
            {
                result.SetNamedValue("Settings", Settings.Export());
            }

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

        protected void NotifyStateChanged()
        {
            _apiController?.NotifyStateChanged(this);
        }

        protected virtual void HandleApiCommand(IApiContext apiContext)
        {
        }

        protected virtual void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = ExportStatusToJsonObject();
        }
    }
}