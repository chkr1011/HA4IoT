using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : IActuator, IStatusProvider
    {
        private bool _isEnabled = true;

        protected ActuatorBase(ActuatorId id, IHttpRequestController httpApi, INotificationHandler logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (httpApi == null) throw new ArgumentNullException(nameof(httpApi));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Id = id;
            Logger = logger;
            HttpApi = httpApi;

            string configurationFilename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actuators", id.Value, "Configuration.json"); ;
            Configuration = new PersistedConfiguration(configurationFilename, logger);
            Configuration.SetValue("type", GetType().FullName);
            
            ExposeToApi();
        }

        public ActuatorId Id { get; }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            set
            {
                if (_isEnabled == value)
                {
                    return;
                }

                _isEnabled = value;
                IsEnabledChanged?.Invoke(this, new ActuatorIsEnabledChangedEventArgs(!value, value));
            }
        }

        protected INotificationHandler Logger { get; }

        protected IHttpRequestController HttpApi { get; }

        public PersistedConfiguration Configuration { get; }

        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;

        public virtual void HandleApiPost(ApiRequestContext context)
        {
            if (context.Request.ContainsKey("isEnabled"))
            {
                IsEnabled = context.Request.GetNamedBoolean("isEnabled", false);
                Logger.Info(Id + ": " + (IsEnabled ? "Enabled" : "Disabled"));
                return;
            }

            Configuration.Update(context.Request);
        }

        private void HandleApiConfigurationPost(HttpContext httpContext)
        {
            JsonObject configuration;
            if (!JsonObject.TryParse(httpContext.Request.Body, out configuration))
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            Configuration.Update(configuration);
        }

        // TODO: Consider creating a ApiHandler which has "GetConfiguration", "GetStatus", "SetConfiguration" "Update"

        public virtual JsonObject GetStatusForApi()
        {
            var result = new JsonObject();
            result.SetNamedValue("isEnabled", JsonValue.CreateBooleanValue(IsEnabled));

            return result;
        }
        
        public virtual JsonObject GetConfigurationForApi()
        {
            return Configuration.GetAsJson();
        }

        private void ExposeToApi()
        {
            HttpApi.Handle(HttpMethod.Post, "configuration").WithSegment(Id.Value).Using(HandleApiConfigurationPost);

            HttpApi.Handle(HttpMethod.Post, "actuator")
                .WithSegment(Id.Value)
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

            HttpApi.Handle(HttpMethod.Get, "actuator")
                .WithSegment(Id.Value)
                .Using(c =>
                {
                    c.Response.Body = new JsonBody(GetStatusForApi());
                });
        }
    }
}