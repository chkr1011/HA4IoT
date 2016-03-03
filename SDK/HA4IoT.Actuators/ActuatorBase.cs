using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase<TSettings> : IActuator, IStatusProvider where TSettings : IActuatorSettings
    {
        protected ActuatorBase(ActuatorId id, IHttpRequestController httpApiController, ILogger logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Id = id;
            Logger = logger;
            HttpApiController = httpApiController;
        }

        public ActuatorId Id { get; }

        protected ILogger Logger { get; }

        protected IHttpRequestController HttpApiController { get; }

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

        public virtual void HandleApiPost(ApiRequestContext context)
        {
        }

        public void ExposeToApi()
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