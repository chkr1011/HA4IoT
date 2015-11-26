using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Core;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : IActuator, IStatusProvider
    {
        private bool _isEnabled = true;

        protected ActuatorBase(ActuatorId id, IHttpRequestController api, INotificationHandler log)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (api == null) throw new ArgumentNullException(nameof(api));
            if (log == null) throw new ArgumentNullException(nameof(log));

            Id = id;
            Log = log;
            Api = api;

            string configurationFilename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actuators", id.Value, "Configuration.json"); ;
            Configuration = new PersistedConfiguration(configurationFilename);
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

        protected INotificationHandler Log { get; }

        protected IHttpRequestController Api { get; }

        public PersistedConfiguration Configuration { get; }

        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;

        public virtual void HandleApiPost(ApiRequestContext context)
        {
            if (context.Request.ContainsKey("isEnabled"))
            {
                IsEnabled = context.Request.GetNamedBoolean("isEnabled", false);
                ControllerBase.Log.Info(Id + ": " + (IsEnabled ? "Enabled" : "Disabled"));
                return;
            }

            Configuration.Update(context.Request);
        }

        public virtual void HandleApiGet(ApiRequestContext context)
        {
            context.Response.SetNamedValue("isEnabled", JsonValue.CreateBooleanValue(IsEnabled)); ;
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

        public virtual JsonObject GetStatus()
        {
            var result = new JsonObject();
            result.SetNamedValue("isEnabled", JsonValue.CreateBooleanValue(IsEnabled));

            return result;
        }
        
        public virtual JsonObject GetConfiguration()
        {
            return Configuration.GetAsJson();
        }

        private void ExposeToApi()
        {
            Api.Handle(HttpMethod.Post, "configuration").WithSegment(Id.Value).Using(HandleApiConfigurationPost);

            Api.Handle(HttpMethod.Post, "actuator")
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

            Api.Handle(HttpMethod.Get, "actuator")
                .WithSegment(Id.Value)
                .Using(c =>
                {
                    JsonObject requestData;
                    if (!JsonObject.TryParse(c.Request.Body, out requestData))
                    {
                        requestData = new JsonObject();
                    }

                    var context = new ApiRequestContext(requestData, new JsonObject());
                    HandleApiGet(context);

                    c.Response.Body = new JsonBody(context.Response);
                });
        }
    }
}