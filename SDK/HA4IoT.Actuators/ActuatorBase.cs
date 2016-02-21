using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : IActuator, IStatusProvider
    {
        protected ActuatorBase(ActuatorId id, IHttpRequestController httpApiController, ILogger logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Id = id;
            Logger = logger;
            HttpApiController = httpApiController;

            Settings = new ActuatorSettings(id, logger);

            ExposeToApi();
        }

        public ActuatorId Id { get; }

        protected ILogger Logger { get; }

        protected IHttpRequestController HttpApiController { get; }

        public IActuatorSettings Settings { get; }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            return Settings.ExportToJsonObject();
        }

        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var configuration = Settings.ExportToJsonObject();
            configuration.SetNamedValue("Type", GetType().FullName.ToJsonValue());

            return configuration;
        }

        public void LoadSettings()
        {
            Settings?.Load();
        }

        public virtual void HandleApiPost(ApiRequestContext context)
        {
        }

        private void ExposeToApi()
        {
            new ActuatorSettingsHttpApiDispatcher(Settings, HttpApiController).ExposeToApi();
            
            HttpApiController.HandlePost($"actuator/{Id.Value}/status")
                .Using(c =>
                {
                    JsonObject requestData;
                    if (!JsonObject.TryParse(c.Request.Body, out requestData))
                    {
                        c.Response.StatusCode = HttpStatusCode.BadRequest;
                        return;
                    }

                    var apiContext = new ApiRequestContext(requestData, new JsonObject());
                    HandleApiPost(apiContext);

                    c.Response.Body = new JsonBody(apiContext.Response);
                });

            HttpApiController.HandleGet($"actuator/{Id.Value}/status")
                .Using(c =>
                {
                    c.Response.Body = new JsonBody(ExportStatusToJsonObject());
                });
        }
    }
}