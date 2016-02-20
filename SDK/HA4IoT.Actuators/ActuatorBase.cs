using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
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

        private void ExposeToApi()
        {
            new ActuatorSettingsHttpApiDispatcher(Settings, HttpApiController).ExposeToApi();
            
            // TODO: Add /settings.
            HttpApiController.Handle(HttpMethod.Post, $"actuator/{Id.Value}")
                .WithRequiredJsonBody()
                .Using(c =>
                {
                    JsonObject requestData;
                    if (!JsonObject.TryParse(c.Request.Body, out requestData))
                    {
                        c.Response.StatusCode = HttpStatusCode.BadRequest;
                        return;
                    }

                    var context = new ApiRequestContext(requestData, new JsonObject());
                    HandleApiPost(context);

                    c.Response.Body = new JsonBody(context.Response);
                });

            HttpApiController.Handle(HttpMethod.Get, $"actuator/{Id.Value}/status")
                .Using(c =>
                {
                    c.Response.Body = new JsonBody(ExportStatusToJsonObject());
                });
        }

        public virtual void HandleApiPost(ApiRequestContext context)
        {
            if (context.Request.ContainsKey("isEnabled"))
            {
                Settings.IsEnabled.Value = context.Request.GetNamedBoolean("isEnabled", false);
                Logger.Info(Id + ": " + (Settings.IsEnabled.Value ? "Enabled" : "Disabled"));
            }
        }
    }
}