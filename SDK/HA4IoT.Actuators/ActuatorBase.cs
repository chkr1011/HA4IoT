using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase<TSettings> : IActuator, IStatusProvider where TSettings : IActuatorSettings
    {
        protected ActuatorBase(ActuatorId id, IApiController apiController, ILogger logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Id = id;
            Logger = logger;
            ApiController = apiController;
        }

        public ActuatorId Id { get; }

        protected ILogger Logger { get; }

        protected IApiController ApiController { get; }

        public TSettings Settings { get; protected set; }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            return Settings.ExportToJsonObject();
        }

        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedValue("Type", GetType().FullName.ToJsonValue());

            if (Settings != null)
            {
                result.SetNamedValue("Settings", Settings.ExportToJsonObject());
            }

            return result;
        }

        public void LoadSettings()
        {
            Settings?.Load();
        }

        protected virtual void HandleApiCommand(IApiContext apiContext)
        {
        }

        protected virtual void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = ExportStatusToJsonObject();
        }

        public void ExposeToApi()
        {
            new ActuatorSettingsApiDispatcher(Settings, ApiController).ExposeToApi();
            
            ApiController.RouteCommand($"actuator/{Id}/status", HandleApiCommand);
            ApiController.RouteRequest($"actuator/{Id}/status", HandleApiRequest);
        }
    }
}